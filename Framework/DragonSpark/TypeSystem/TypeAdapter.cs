using DragonSpark.Aspects;
using DragonSpark.Extensions;
using DragonSpark.Runtime.Specifications;
using DragonSpark.Sources.Parameterized;
using DragonSpark.Sources.Parameterized.Caching;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;

namespace DragonSpark.TypeSystem
{
	public sealed class TypeAdapter
	{
		readonly static Func<Type, bool> Specification = ApplicationTypeSpecification.Default.ToSpecificationDelegate();
		readonly static Func<Type, IEnumerable<Type>> Expand = ExpandInterfaces;
		readonly Func<Type, bool> isAssignableFrom;
		
		readonly Func<Type, ImmutableArray<MethodMapping>> methodMapper;
		readonly Func<Type, Type[]> getTypeArguments;
		public TypeAdapter( Type type ) : this( type, type.GetTypeInfo() ) {}

		public TypeAdapter( TypeInfo info ) : this( info.AsType(), info ) {}

		public TypeAdapter( Type type, TypeInfo info )
		{
			Type = type;
			Info = info;
			methodMapper = new DecoratedSourceCache<Type, ImmutableArray<MethodMapping>>( new MethodMapper( this ).Create ).Get;
			GenericFactoryMethods = new GenericStaticMethodFactories( Type );
			GenericCommandMethods = new GenericStaticMethodCommands( Type );
			isAssignableFrom = new IsInstanceOfTypeOrDefinitionCache( this ).Get;
			getTypeArguments = new GetTypeArgumentsForCache( this ).Get;
		}

		public Type Type { get; }

		public TypeInfo Info { get; }

		public GenericStaticMethodFactories GenericFactoryMethods { get; }
		public GenericStaticMethodCommands GenericCommandMethods { get; }

		public Type[] WithNested() => Info.Append( Info.DeclaredNestedTypes ).AsTypes().Where( Specification ).ToArray();

		public ConstructorInfo FindConstructor( params Type[] parameterTypes ) => 
				InstanceConstructors.Default.Get( Info )
				.Introduce( parameterTypes, tuple => CompatibleArgumentsSpecification.Default.Get( tuple.Item1 ).IsSatisfiedBy( tuple.Item2 ) )
				.SingleOrDefault();

		public bool IsAssignableFrom( Type other ) => isAssignableFrom( other );
		bool IsAssignableFromBody( Type parameter ) => Info.IsGenericTypeDefinition && parameter.Adapt().IsGenericOf( Type ) || Info.IsAssignableFrom( parameter.GetTypeInfo() );
		class IsInstanceOfTypeOrDefinitionCache : ArgumentCache<Type, bool>
		{
			public IsInstanceOfTypeOrDefinitionCache( TypeAdapter owner ) : base( owner.IsAssignableFromBody ) {}
		}

		public bool IsInstanceOfType( object instance ) => IsAssignableFrom( instance.GetType() );
		
		public Assembly Assembly => Info.Assembly;

		public Type[] GetHierarchy( bool includeRoot = true )
		{
			var builder = ImmutableArray.CreateBuilder<Type>();
			builder.Add( Type );
			var current = Info.BaseType;
			while ( current != null )
			{
				if ( current != typeof(object) || includeRoot )
				{
					builder.Add( current );
				}
				current = current.GetTypeInfo().BaseType;
			}
			var result = builder.ToArray();
			return result;
		}

		public Type GetEnumerableType() => InnerType( Type, types => types.FirstOrDefault(), i => i.Adapt().IsGenericOf( typeof(IEnumerable<>) ) );

		public Type GetInnerType() => InnerType( Type, types => types.Only() );

		static Type InnerType( Type target, Func<Type[], Type> fromGenerics, Func<TypeInfo, bool> check = null )
		{
			var info = target.GetTypeInfo();
			var result = info.IsGenericType && info.GenericTypeArguments.Any() && ( check == null || check( info ) ) ? fromGenerics( info.GenericTypeArguments ) :
				target.IsArray ? target.GetElementType() : null;
			return result;
		}

		public Type[] GetTypeArgumentsFor( Type implementationType ) => getTypeArguments( implementationType );
		Type[] GetTypeArgumentsForBody( Type implementationType ) => GetImplementations( implementationType ).First().GenericTypeArguments;
		class GetTypeArgumentsForCache : ArgumentCache<Type, Type[]>
		{
			public GetTypeArgumentsForCache( TypeAdapter owner ) : base( owner.GetTypeArgumentsForBody ) {}
		}

		[Freeze]
		public Type[] GetImplementations( Type genericDefinition, bool includeInterfaces = true )
		{
			var result = Type.Append( includeInterfaces ? Expand( Type ) : Items<Type>.Default )
							 .Distinct()
							 .Introduce( genericDefinition, tuple =>
															{
																var first = tuple.Item1.GetTypeInfo();
																var second = tuple.Item2.GetTypeInfo();
																var match = first.IsGenericType && second.IsGenericType && tuple.Item1.GetGenericTypeDefinition() == tuple.Item2.GetGenericTypeDefinition();
																return match;
															} )
							 .Fixed();
			return result;
		}

		public ImmutableArray<MethodMapping> GetMappedMethods<T>() => GetMappedMethods( typeof(T) );
		public ImmutableArray<MethodMapping> GetMappedMethods( Type interfaceType ) => methodMapper( interfaceType );
		

		[Freeze]
		public bool IsGenericOf( Type genericDefinition ) => IsGenericOf( genericDefinition, true );

		[Freeze]
		public bool IsGenericOf( Type genericDefinition, bool includeInterfaces ) => GetImplementations( genericDefinition, includeInterfaces ).Any();

		[Freeze]
		public Type[] GetAllInterfaces() => Expand( Type ).ToArray();

		static IEnumerable<Type> ExpandInterfaces( Type target ) => target.Append( target.GetTypeInfo().ImplementedInterfaces.SelectMany( Expand ) ).Where( x => x.GetTypeInfo().IsInterface ).Distinct();

		[Freeze]
		public Type[] GetEntireHierarchy() => Expand( Type ).Union( GetHierarchy( false ) ).Distinct().ToArray();
	}

	class MethodMapper//  : FactoryBase<Type, ImmutableArray<MethodMapping>>
	{
		readonly TypeAdapter adapter;

		public MethodMapper( TypeAdapter adapter )
		{
			this.adapter = adapter;
		}

		public ImmutableArray<MethodMapping> Create( Type parameter )
		{
			var generic = parameter.GetTypeInfo().IsGenericTypeDefinition ? adapter.GetImplementations( parameter ).FirstOrDefault() : null;
			var implementation = generic ?? ( parameter.Adapt().IsAssignableFrom( adapter.Type ) ? parameter : null );
			if ( implementation != null )
			{
				var map = adapter.Info.GetRuntimeInterfaceMap( implementation );
				var result = map.InterfaceMethods.Tuple( map.TargetMethods ).Select( tuple => new MethodMapping( tuple.Item1, tuple.Item2 ) ).ToImmutableArray();
				return result;
			}
			return Items<MethodMapping>.Immutable;
		}
	}

	public struct MethodMapping
	{
		public MethodMapping( MethodInfo interfaceMethod, MethodInfo mappedMethod )
		{
			InterfaceMethod = interfaceMethod;
			MappedMethod = mappedMethod;
		}

		public MethodInfo InterfaceMethod { get; }
		public MethodInfo MappedMethod { get; }
	}

	class InstanceConstructors : ArgumentCache<TypeInfo, ImmutableArray<ConstructorInfo>>
	{
		public static InstanceConstructors Default { get; } = new InstanceConstructors();
		InstanceConstructors() : base( info => info.DeclaredConstructors.Where( constructorInfo => constructorInfo.IsPublic && !constructorInfo.IsStatic ).ToImmutableArray() ) {}
	}
}