using DragonSpark.Activation;
using DragonSpark.Aspects;
using DragonSpark.Diagnostics;
using DragonSpark.Extensions;
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
		readonly ICache<MethodDescriptor, MethodInfo> methods;

		public TypeAdapter( [Required]Type type ) : this( type, type.GetTypeInfo() ) {}

		public TypeAdapter( [Required]TypeInfo info ) : this( info.AsType(), info ) {}

		public TypeAdapter( [Required]Type type,  [Required]TypeInfo info )
		{
			Type = type;
			Info = info;
			methods = new ConcurrentEqualityCache<MethodDescriptor, MethodInfo>( new GenericMethodFactory( Type ).ToDelegate() );
		}

		public Type Type { get; }

		public TypeInfo Info { get; }

		public Type[] WithNested() => Info.Append( Info.DeclaredNestedTypes ).AsTypes().Where( ApplicationTypeSpecification.Instance.ToDelegate() ).ToArray();

		public bool IsDefined<T>( [Required] bool inherited = false ) where T : Attribute => Info.IsDefined( typeof(T), inherited );

		public ConstructorInfo FindConstructor( params object[] parameters ) => FindConstructor( parameters.Select( o => o?.GetType() ).ToArray() );

		public ConstructorInfo FindConstructor( params Type[] parameterTypes ) => 
			Info.DeclaredConstructors
				.Introduce( parameterTypes, tuple => tuple.Item1.IsPublic && !tuple.Item1.IsStatic && CompatibleArgumentsSpecification.Default.Get( tuple.Item1 ).IsSatisfiedBy( tuple.Item2 ) )
				.SingleOrDefault();

		public object Invoke( string methodName, IEnumerable<Type> types ) => Invoke( methodName, types, Items<object>.Default );

		public object Invoke( string methodName, IEnumerable<Type> types, params object[] parameters ) => Invoke( null, methodName, types, parameters );

		public object Invoke( object instance, string methodName, IEnumerable<Type> types, params object[] parameters ) => Invoke<object>( instance, methodName, types, parameters );

		public T Invoke<T>( string methodName, IEnumerable<Type> types, params object[] parameters ) => Invoke<T>( null, methodName, types, parameters );

		public T Invoke<T>( object instance, string methodName, IEnumerable<Type> types, params object[] parameters )
		{
			var descriptor = new MethodDescriptor( methodName, types, parameters );
			var methodInfo = methods.Get( descriptor );
			var result = (T)methodInfo.Invoke( methodInfo.IsStatic ? null : instance, parameters );
			return result;
		}

		class GenericMethodFactory : FactoryBase<MethodDescriptor, MethodInfo>
		{
			readonly static Func<ValueTuple<MethodInfo, MethodDescriptor>, MethodInfo> CreateSelector = Create;

			readonly Type type;

			public GenericMethodFactory( Type type )
			{
				this.type = type;
			}

			public override MethodInfo Create( MethodDescriptor parameter )
			{
				var result = type.GetRuntimeMethods()
								 .Introduce( parameter, tuple => GenericMethodEqualitySpecification.Default.Get( tuple.Item1 ).IsSatisfiedBy( tuple.Item2 ), CreateSelector )
								 .Introduce( parameter, tuple => CompatibleArgumentsSpecification.Default.Get( tuple.Item1 ).IsSatisfiedBy( tuple.Item2.ParameterTypes ) )
								 .SingleOrDefault();
				return result;
			}

			static MethodInfo Create( ValueTuple<MethodInfo, MethodDescriptor> item )
			{
				try
				{
					return item.Item1.MakeGenericMethod( item.Item2.GenericTypes );
				}
				catch ( ArgumentException e )
				{
					DiagnosticProperties.Logger.Get( typeof(TypeAdapter) ).Verbose( e, "Could not create a generic method for {Method} with types {Types}", item.Item1, item.Item2.GenericTypes );
					return item.Item1;
				}
			}
		}

		class CompatibleArgumentsSpecification : SpecificationWithContextBase<Type[], ParameterInfo[]>
		{
			public static ICache<MethodBase, ISpecification<Type[]>> Default { get; } = new Cache<MethodBase, ISpecification<Type[]>>( method => new CompatibleArgumentsSpecification( method ) );

			readonly static Func<ValueTuple<ParameterInfo, Type[]>, int, bool> SelectCompatible = Compatible;
			CompatibleArgumentsSpecification( MethodBase context ) : base( context.GetParameters() ) {}
			
			public override bool IsSatisfiedBy( Type[] parameter )
			{
				var result = 
					parameter.Length >= Context.Count( info => !info.IsOptional ) && 
					parameter.Length <= Context.Length && 
					Context
						.Introduce( parameter )
						.Select( SelectCompatible )
						.All();
				return result;
			}

			static bool Compatible( ValueTuple<ParameterInfo, Type[]> context, int i )
			{
				var type = context.Item2.ElementAtOrDefault( i );
				var result = type != null ? context.Item1.ParameterType.Adapt().IsAssignableFrom( type ) : i < context.Item2.Length || context.Item1.IsOptional;
				return result;
			}
		}

		class GenericMethodEqualitySpecification : SpecificationWithContextBase<MethodDescriptor, MethodBase>
		{
			public static ICache<MethodBase, ISpecification<MethodDescriptor>> Default { get; } = new Cache<MethodBase, ISpecification<MethodDescriptor>>( method => new GenericMethodEqualitySpecification( method ) );
			GenericMethodEqualitySpecification( MethodBase method ) : base( method ) {}

			public override bool IsSatisfiedBy( MethodDescriptor parameter ) => 
				Context.IsGenericMethod
				&&
				Context.Name == parameter.Name 
				&& 
				Context.GetGenericArguments().Length == parameter.GenericTypes.Length;
		}

		public struct MethodDescriptor : IEquatable<MethodDescriptor>
		{
			readonly int code;
			public MethodDescriptor( string name, IEnumerable<Type> genericTypes, params object[] parameters ) : this( name, genericTypes.Fixed(), parameters.Select( o => o?.GetType() ).ToArray() ) {}

			MethodDescriptor( string name, Type[] genericTypes, Type[] parameterTypes )
			{
				Name = name;
				GenericTypes = genericTypes;
				ParameterTypes = parameterTypes;
				code = Hash.CombineValues( EnumerableEx.Return<object>( Name ).Concat( GenericTypes ).Concat( ParameterTypes ).ToImmutableArray() );
			}

			public string Name { get; }
			public Type[] GenericTypes { get; }
			public Type[] ParameterTypes { get; }

			public bool Equals( MethodDescriptor other ) => code == other.code;

			public override bool Equals( object obj )
			{
				return !ReferenceEquals( null, obj ) && ( obj is MethodDescriptor && Equals( (MethodDescriptor)obj ) );
			}

			public override int GetHashCode() => code;

			public static bool operator ==( MethodDescriptor left, MethodDescriptor right ) => left.Equals( right );

			public static bool operator !=( MethodDescriptor left, MethodDescriptor right ) => !left.Equals( right );
		}

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

		public bool IsGenericOf<T>( bool includeInterfaces = true ) => IsGenericOf( typeof(T).GetGenericTypeDefinition(), includeInterfaces );

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

		Type CheckGeneric( Type type )
		{
			if ( type.GetTypeInfo().IsGenericTypeDefinition )
			{
				var implementations = GetImplementations( type );
				if ( implementations.Any() )
				{
					return implementations.First();
				}
			}
			return null;
		}

		public bool IsGenericOf( Type genericDefinition ) => IsGenericOf( genericDefinition, true );

		public bool IsGenericOf( Type genericDefinition, bool includeInterfaces ) => GetImplementations( genericDefinition, includeInterfaces ).Any();

		public Type[] GetAllInterfaces() => ExpandInterfaces( Type ).ToArray();

		static IEnumerable<Type> ExpandInterfaces( Type target ) => target.Append( target.GetTypeInfo().ImplementedInterfaces.SelectMany( ExpandInterfaces ) ).Where( x => x.GetTypeInfo().IsInterface ).Distinct();

		public Type[] GetEntireHierarchy() => ExpandInterfaces( Type ).Union( GetHierarchy( false ) ).Distinct().ToArray();
	}
}