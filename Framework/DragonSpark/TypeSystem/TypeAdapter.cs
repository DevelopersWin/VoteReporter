using DragonSpark.Application;
using DragonSpark.Aspects;
using DragonSpark.Expressions;
using DragonSpark.Extensions;
using DragonSpark.Sources.Coercion;
using DragonSpark.Sources.Parameterized;
using DragonSpark.Sources.Parameterized.Caching;
using DragonSpark.Specifications;
using DragonSpark.TypeSystem.Generics;
using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;

namespace DragonSpark.TypeSystem
{
	public sealed class TypeAssignableSpecification<T> : DelegatedSpecification<Type>
	{
		public static ISpecification<Type> Default { get; } = new TypeAssignableSpecification<T>();
		TypeAssignableSpecification() : base( TypeAssignableSpecification.Delegates.Get( typeof(T) ) ) {}
	}

	public sealed class TypeAssignableSpecification : SpecificationCache<Type>
	{
		public static TypeAssignableSpecification Default { get; } = new TypeAssignableSpecification();
		public static IParameterizedSource<Type, Func<Type, bool>> Delegates { get; } = new Cache<Type, Func<Type, bool>>( Default.To( SpecificationDelegates<Type>.Default ).Get );
		TypeAssignableSpecification() : base( type => new DefaultImplementation( type ).ToCachedSpecification() ) {}

		sealed class DefaultImplementation : TypeSpecificationBase
		{
			public DefaultImplementation( Type context ) : base( context ) {}

			public override bool IsSatisfiedBy( Type parameter ) => 
				Info.IsGenericTypeDefinition && parameter.Adapt().IsGenericOf( Context ) || Info.IsAssignableFrom( parameter.GetTypeInfo() ) || Nullable.GetUnderlyingType( parameter ) == Context;
		}
	}

	public abstract class TypeSpecificationBase : SpecificationWithContextBase<Type>
	{
		protected TypeSpecificationBase( Type context ) : this( context, context.GetTypeInfo() ) {}

		[UsedImplicitly]
		protected TypeSpecificationBase( Type context, TypeInfo info ) : base( context )
		{
			Info = info;
		}

		protected TypeInfo Info { get; }
	}

	public static partial class Extensions
	{
		public static IGenericMethodContext<Invoke> GetFactory( this Type @this, string methodName ) => GenericFactories.Default.Get( @this )[methodName];
		sealed class GenericFactories : DecoratedCache<Type, GenericStaticMethodFactories>
		{
			public static GenericFactories Default { get; } = new GenericFactories();
			GenericFactories() {}
		}

		public static IGenericMethodContext<Execute> GetCommand( this Type @this, string methodName ) => GenericCommands.Default.Get( @this )[methodName];
		sealed class GenericCommands : DecoratedCache<Type, GenericStaticMethodCommands>
		{
			public static GenericCommands Default { get; } = new GenericCommands();
			GenericCommands() {}
		}

		public static ImmutableArray<Type> WithNested( this Type @this ) => NestedTypesFactory.Default.Get( @this );

		public static bool IsAssignableFrom( this Type @this, Type other ) => TypeAssignableSpecification.Default.Get( @this ).IsSatisfiedBy( other );
		
		public static bool IsInstanceOfType( this Type @this, object instance ) => @this.IsAssignableFrom( instance.GetType() );
	}

	public sealed class NestedTypesFactory : CacheWithImplementedFactoryBase<TypeInfo, ImmutableArray<Type>>
	{
		readonly static Func<TypeInfo, bool> Specification = ApplicationTypeSpecification.Default.IsSatisfiedBy;

		public static NestedTypesFactory Default { get; } = new NestedTypesFactory();
		NestedTypesFactory() {}

		protected override ImmutableArray<Type> Create( TypeInfo parameter ) =>
			parameter.Append( parameter.DeclaredNestedTypes ).Where( Specification ).AsTypes().ToImmutableArray();
	}

	public sealed class ConstructorLocator : ParameterizedSourceCache<TypeInfo, ImmutableArray<Type>, ConstructorInfo>
	{
		public static ConstructorLocator Default { get; } = new ConstructorLocator();
		ConstructorLocator() : base( info => new ExtendedDictionaryCache<ImmutableArray<Type>, ConstructorInfo>( new DefaultImplementation( info ).Get ) ) {}

		sealed class DefaultImplementation : ParameterizedSourceBase<ImmutableArray<Type>, ConstructorInfo>
		{
			ImmutableArray<ConstructorInfo> candidates;

			public DefaultImplementation( TypeInfo typeInfo ) : this( InstanceConstructors.Default.Get( typeInfo ) ) {}

			DefaultImplementation( ImmutableArray<ConstructorInfo> candidates )
			{
				this.candidates = candidates;
			}

			public override ConstructorInfo Get( ImmutableArray<Type> parameter ) =>
				candidates
					.Introduce( parameter, tuple => CompatibleArgumentsSpecification.Default.Get( tuple.Item1 ).IsSatisfiedBy( tuple.Item2 ) )
					.SingleOrDefault();
		}
	}

	public sealed class TypeAdapter : ITypeAware
	{
		readonly static Func<Type, IEnumerable<Type>> Expand = ExpandInterfaces;
		// readonly Func<Type, bool> isAssignableFrom;
		
		readonly Func<Type, ImmutableArray<MethodMapping>> methodMapper;
		public TypeAdapter( Type referencedType ) : this( referencedType, referencedType.GetTypeInfo() ) {}

		public TypeAdapter( Type referencedType, TypeInfo info )
		{
			ReferencedType = referencedType;
			Info = info;
			methodMapper = new DecoratedSourceCache<Type, ImmutableArray<MethodMapping>>( new MethodMapper( this ).Get ).Get;
		}

		public Type ReferencedType { get; }

		public TypeInfo Info { get; }

		public IEnumerable<Type> GetHierarchy( bool includeRoot = false )
		{
			yield return ReferencedType;
			var current = Info.BaseType;
			while ( current != null )
			{
				if ( current != typeof(object) || includeRoot )
				{
					yield return current;
				}
				current = current.GetTypeInfo().BaseType;
			}
		}

		[Freeze]
		public Type GetEnumerableType() => InnerType( GetHierarchy(), types => types.Only(), i => i.Adapt().IsGenericOf( typeof(IEnumerable<>) ) );

		[Freeze]
		public Type GetInnerType() => InnerType( GetHierarchy(), types => types.Only() );

		static Type InnerType( IEnumerable<Type> hierarchy, Func<Type[], Type> fromGenerics, Func<TypeInfo, bool> check = null )
		{
			foreach ( var type in hierarchy )
			{
				var info = type.GetTypeInfo();
				var result = info.IsGenericType && info.GenericTypeArguments.Any() && ( check?.Invoke( info ) ?? true ) ? fromGenerics( info.GenericTypeArguments ) :
					type.IsArray ? type.GetElementType() : null;
				if ( result != null )
				{
					return result;
				}
			}
			return null;
		}

		[Freeze]
		public Type[] GetImplementations( Type genericDefinition, bool includeInterfaces = true )
		{
			var result = ReferencedType.Append( includeInterfaces ? Expand( ReferencedType ) : Items<Type>.Default )
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

		public ImmutableArray<MethodMapping> GetMappedMethods( Type interfaceType ) => methodMapper( interfaceType );
		

		[Freeze]
		public bool IsGenericOf( Type genericDefinition ) => IsGenericOf( genericDefinition, true );

		[Freeze]
		public bool IsGenericOf( Type genericDefinition, bool includeInterfaces ) => GetImplementations( genericDefinition, includeInterfaces ).Any();

		[Freeze]
		public Type[] GetAllInterfaces() => Expand( ReferencedType ).ToArray();

		static IEnumerable<Type> ExpandInterfaces( Type target ) => target.Append( target.GetTypeInfo().ImplementedInterfaces.SelectMany( Expand ) ).Where( x => x.GetTypeInfo().IsInterface ).Distinct();
	}
}