using DragonSpark.Activation;
using DragonSpark.Aspects;
using DragonSpark.Extensions;
using DragonSpark.Runtime;
using DragonSpark.Runtime.Properties;
using DragonSpark.Runtime.Specifications;
using PostSharp.Patterns.Contracts;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;

namespace DragonSpark.TypeSystem
{
	public class TypeAdapter
	{
		public TypeAdapter( [Required]Type type ) : this( type, type.GetTypeInfo() ) {}

		public TypeAdapter( [Required]TypeInfo info ) : this( info.AsType(), info ) {}

		public TypeAdapter( [Required]Type type,  [Required]TypeInfo info )
		{
			Type = type;
			Info = info;
			GenericMethods = new GenericMethodInvoker( Type );
		}

		public Type Type { get; }

		public TypeInfo Info { get; }

		public GenericMethodInvoker GenericMethods { get; }

		public Type[] WithNested() => Info.Append( Info.DeclaredNestedTypes ).AsTypes().Where( ApplicationTypeSpecification.Instance.ToDelegate() ).ToArray();

		// public bool IsDefined<T>( [Required] bool inherited = false ) where T : Attribute => Info.IsDefined( typeof(T), inherited );

		public ConstructorInfo FindConstructor( params object[] parameters ) => FindConstructor( ObjectTypeFactory.Instance.Create( parameters ) );

		public ConstructorInfo FindConstructor( params Type[] parameterTypes ) => 
			Info.DeclaredConstructors
				.Introduce( parameterTypes, tuple => tuple.Item1.IsPublic && !tuple.Item1.IsStatic && CompatibleArgumentsSpecification.Default.Get( tuple.Item1 ).IsSatisfiedBy( tuple.Item2 ) )
				.SingleOrDefault();

		public bool IsAssignableFrom( TypeInfo other ) => IsAssignableFrom( other.AsType() );

		public bool IsAssignableFrom( Type other ) => Info.IsAssignableFrom( other.GetTypeInfo() ) /*|| GetCaster( other ) != null*/;

		public bool IsInstanceOfType( object context ) => IsAssignableFrom( context.GetType() );

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

		// public Type GetResultType() => type.Append( ExpandInterfaces( type ) ).FirstWhere( t => InnerType( t, types => types.LastOrDefault() ) );

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

		// public Type[] GetImplementations<T>( bool includeInterfaces = true ) => GetImplementations( typeof(T).GetGenericTypeDefinition(), includeInterfaces );

		[Freeze]
		public Type[] GetImplementations( Type genericDefinition, bool includeInterfaces = true ) =>
			Type.Append( includeInterfaces ? ExpandInterfaces( Type ) : Items<Type>.Default )
				.Introduce( genericDefinition, tuple => tuple.Item1.GetTypeInfo().IsGenericType && tuple.Item2.GetTypeInfo().IsGenericType && tuple.Item1.GetGenericTypeDefinition() == tuple.Item2.GetGenericTypeDefinition() )
				.Fixed();

		[Freeze]
		public Tuple<MethodInfo, MethodInfo>[] GetMappedMethods( Type implementedType )
		{
			var implementation = CheckGeneric( implementedType ) ?? ( implementedType.Adapt().IsAssignableFrom( Type ) ? implementedType : null );
			if ( implementation != null )
			{
				var map = Info.GetRuntimeInterfaceMap( implementation );
				var result = map.InterfaceMethods.Tuple( map.TargetMethods ).Fixed();
				return result;
			}
			return Items<Tuple<MethodInfo, MethodInfo>>.Default;
		}

		Type CheckGeneric( Type type ) => type.GetTypeInfo().IsGenericTypeDefinition ? GetImplementations( type ).FirstOrDefault() : null;

		public bool IsGenericOf<T>( bool includeInterfaces = true ) => IsGenericOf( typeof(T).GetGenericTypeDefinition(), includeInterfaces );

		[Freeze]
		public bool IsGenericOf( Type genericDefinition ) => IsGenericOf( genericDefinition, true );

		[Freeze]
		public bool IsGenericOf( Type genericDefinition, bool includeInterfaces ) => GetImplementations( genericDefinition, includeInterfaces ).Any();

		[Freeze]
		public Type[] GetAllInterfaces() => ExpandInterfaces( Type ).ToArray();

		static IEnumerable<Type> ExpandInterfaces( Type target ) => target.Append( target.GetTypeInfo().ImplementedInterfaces.SelectMany( ExpandInterfaces ) ).Where( x => x.GetTypeInfo().IsInterface ).Distinct();

		[Freeze]
		public Type[] GetEntireHierarchy() => ExpandInterfaces( Type ).Union( GetHierarchy( false ) ).Distinct().ToArray();
	}
}