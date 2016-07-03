using DragonSpark.Activation;
using DragonSpark.Aspects;
using DragonSpark.Extensions;
using DragonSpark.Runtime;
using DragonSpark.Runtime.Properties;
using DragonSpark.Runtime.Specifications;
using PostSharp.Patterns.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DragonSpark.TypeSystem
{
	public class TypeAdapter
	{
		readonly static Func<Type, bool> Specification = ApplicationTypeSpecification.Instance.ToDelegate();
		readonly static Func<Type, IEnumerable<Type>> Expand = ExpandInterfaces;

		public TypeAdapter( [Required]Type type ) : this( type, type.GetTypeInfo() ) {}

		public TypeAdapter( [Required]TypeInfo info ) : this( info.AsType(), info ) {}

		public TypeAdapter( [Required]Type type,  [Required]TypeInfo info )
		{
			Type = type;
			Info = info;
			GenericMethods = new GenericMethodInvoker( Type );
			cache = new IsInstanceOfTypeOrDefinitionCache( this );
		}

		public Type Type { get; }

		public TypeInfo Info { get; }

		public GenericMethodInvoker GenericMethods { get; }

		public Type[] WithNested() => Info.Append( Info.DeclaredNestedTypes ).AsTypes().Where( Specification ).ToArray();

		// public bool IsDefined<T>( [Required] bool inherited = false ) where T : Attribute => Info.IsDefined( typeof(T), inherited );

		public ConstructorInfo FindConstructor( params object[] parameters ) => FindConstructor( ObjectTypeFactory.Instance.Create( parameters ) );

		public ConstructorInfo FindConstructor( params Type[] parameterTypes ) => 
			Info.DeclaredConstructors
				.Introduce( parameterTypes, tuple => tuple.Item1.IsPublic && !tuple.Item1.IsStatic && CompatibleArgumentsSpecification.Default.Get( tuple.Item1 ).IsSatisfiedBy( tuple.Item2 ) )
				.SingleOrDefault();

		// public bool IsAssignableFrom( TypeInfo other ) => IsAssignableFrom( other.AsType() );

		// [Freeze]
		public bool IsAssignableFrom( Type other ) => Info.IsAssignableFrom( other.GetTypeInfo() );

		public bool IsInstanceOfType( object instance ) => IsAssignableFrom( instance.GetType() );

		readonly ICache<Type, bool> cache;
		
		public bool IsInstanceOfTypeOrDefinition( Type type ) => cache.Get( type );
		bool IsInstanceOfTypeOrDefinitionBody( Type parameter ) => Info.IsGenericTypeDefinition && Info.IsAssignableFrom( parameter.GetTypeInfo() ) || IsAssignableFrom( parameter );
		class IsInstanceOfTypeOrDefinitionCache : ArgumentCache<Type, bool>
		{
			public IsInstanceOfTypeOrDefinitionCache( TypeAdapter owner ) : base( owner.IsInstanceOfTypeOrDefinitionBody ) {}
		}

		public Assembly Assembly => Info.Assembly;

		public Type[] GetHierarchy( bool includeRoot = true )
		{
			var builder = ArrayBuilder<Type>.GetInstance();
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
			var result = builder.ToArrayAndFree();
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

		[Freeze]
		public Type[] GetTypeArgumentsFor( Type implementationType, bool includeInterfaces = true ) => GetImplementations( implementationType, includeInterfaces ).First().GenericTypeArguments;

		[Freeze]
		public Type[] GetImplementations( Type genericDefinition, bool includeInterfaces = true )
		{
			var result = Type.Append( includeInterfaces ? Expand( Type ) : Items<Type>.Default )
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

		[Freeze]
		public ValueTuple<MethodInfo, MethodInfo>[] GetMappedMethods( Type implementedType )
		{
			var implementation = CheckGeneric( implementedType ) ?? ( implementedType.Adapt().IsAssignableFrom( Type ) ? implementedType : null );
			if ( implementation != null )
			{
				var map = Info.GetRuntimeInterfaceMap( implementation );
				var result = map.InterfaceMethods.Tuple( map.TargetMethods ).Fixed();
				return result;
			}
			return Items<ValueTuple<MethodInfo, MethodInfo>>.Default;
		}

		Type CheckGeneric( Type type ) => type.GetTypeInfo().IsGenericTypeDefinition ? GetImplementations( type ).FirstOrDefault() : null;

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
}