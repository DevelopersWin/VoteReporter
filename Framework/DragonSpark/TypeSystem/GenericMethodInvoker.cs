using DragonSpark.Activation;
using DragonSpark.Aspects;
using DragonSpark.Diagnostics;
using DragonSpark.Extensions;
using DragonSpark.Runtime;
using DragonSpark.Runtime.Properties;
using DragonSpark.Runtime.Specifications;
using System;
using System.Linq;
using System.Reflection;

namespace DragonSpark.TypeSystem
{
	public class GenericMethodInvoker
	{
		readonly ArgumentCache cache;

		public GenericMethodInvoker( Type type ) : this( new ArgumentCache( type ) ) {}

		GenericMethodInvoker( ArgumentCache cache )
		{
			this.cache = cache;
		}

		public T Invoke<T>( string methodName, Type[] genericTypes, params object[] arguments ) => Invoke<T>( null, methodName, genericTypes, arguments );

		public T Invoke<T>( object instance, string methodName, Type[] genericTypes, params object[] arguments ) => (T)Invoke( instance, methodName, genericTypes, arguments );

		public object Invoke( string methodName, params Type[] genericTypes ) => Invoke( methodName, genericTypes, Items<object>.Default );

		public object Invoke( string methodName, Type[] genericTypes, params object[] arguments ) => Invoke( null, methodName, genericTypes, arguments );

		public object Invoke( object instance, string methodName, Type[] genericTypes, params object[] arguments )
		{
			var key = new MethodDescriptor( methodName, genericTypes, ObjectTypeFactory.Instance.Create( arguments ) );
			var info = cache.Get( key );
			var result = info.Invoke( info.IsStatic ? null : instance, arguments );
			return result;
		}

		class ArgumentCache : ArgumentCache<MethodDescriptor, MethodInfo>
		{
			public ArgumentCache( Type type ) : base( descriptor => descriptor.Key, new Factory( type ).Create ) {}

			class Factory : FactoryBase<MethodDescriptor, MethodInfo>
			{
				readonly static Func<ValueTuple<MethodInfo, MethodDescriptor>, MethodInfo> CreateSelector = Create;

				readonly Type type;

				public Factory( Type type )
				{
					this.type = type;
				}

				public override MethodInfo Create( MethodDescriptor parameter )
				{
					var result = type.GetRuntimeMethods()
									 .Introduce( parameter, tuple => GenericMethodEqualitySpecification.Default.Get( tuple.Item1 ).IsSatisfiedBy( tuple.Item2 ), CreateSelector )
									 .Introduce( parameter, tuple => CompatibleArgumentsSpecification.Default.Get( tuple.Item1 ).IsSatisfiedBy( tuple.Item2.ArgumentTypes ) )
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

			public MethodDescriptor( string name, Type[] genericTypes, Type[] argumentTypes )
			{
				Name = name;
				GenericTypes = genericTypes;
				ArgumentTypes = argumentTypes;
				Key = new object[] { name, genericTypes, argumentTypes };
				code = StructuralEqualityComparer<object[]>.Instance.GetHashCode( Key );
			}

			public string Name { get; }
			public Type[] GenericTypes { get; }
			public Type[] ArgumentTypes { get; }
			public object[] Key { get; }

			public bool Equals( MethodDescriptor other ) => code == other.code;

			public override bool Equals( object obj ) => !ReferenceEquals( null, obj ) && obj is MethodDescriptor && Equals( (MethodDescriptor)obj );

			public override int GetHashCode() => code;

			public static bool operator ==( MethodDescriptor left, MethodDescriptor right ) => left.Equals( right );

			public static bool operator !=( MethodDescriptor left, MethodDescriptor right ) => !left.Equals( right );
		}
	}

	class ObjectTypeFactory : FactoryBase<object[], Type[]>
	{
		public static ObjectTypeFactory Instance { get; } = new ObjectTypeFactory();

		public override Type[] Create( object[] parameter )
		{
			var builder = ArrayBuilder<Type>.GetInstance();
			foreach ( var item in parameter )
			{
				builder.Add( item?.GetType() );
			}
			var result = builder.ToArrayAndFree();
			return result;
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
}