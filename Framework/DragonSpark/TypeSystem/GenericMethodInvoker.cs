using DragonSpark.Activation;
using DragonSpark.Aspects;
using DragonSpark.Diagnostics;
using DragonSpark.Extensions;
using DragonSpark.Runtime.Properties;
using DragonSpark.Runtime.Specifications;
using System;
using System.Linq;
using System.Linq.Expressions;
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
			var @delegate = cache.Get( key );

			var factory = @delegate as Func<object, object[], object>;
			if ( factory != null )
			{
				return factory( instance, arguments );
			}

			( (Action<object, object[]>)@delegate )( instance, arguments );
			return null;
		}

		class InvokeDelegateFactory : Cache<MethodInfo, Delegate>
		{
			public static InvokeDelegateFactory Instance { get; } = new InvokeDelegateFactory();
			InvokeDelegateFactory() : base( Create ) {}

			static Delegate Create( MethodInfo parameter )
			{
				var parameters = parameter.GetParameterTypes();
				var instance = Expression.Parameter( typeof(object), "instance" );
				var arguments = Expression.Parameter( typeof(object[]), "args" );

				var array = new Expression[parameters.Length];
				for ( var i = 0; i < parameters.Length; i++ )
				{
					var index = Expression.ArrayIndex( arguments, Expression.Constant( i ) );
					array[i] = Expression.Convert( index, parameters[i] );
				}

				var body = parameter.IsStatic ? Expression.Call( parameter, array ) : Expression.Call( Expression.Convert( instance, parameter.DeclaringType ), parameter, array );

				var result = Expression.Lambda( body, instance, arguments ).Compile();
				return result;
			}
		}

		class ArgumentCache : ArgumentCache<MethodDescriptor, Delegate>
		{
			public ArgumentCache( Type type ) : base( descriptor => descriptor.Key, new Factory( type ).Create ) {}

			class Factory : FactoryBase<MethodDescriptor, Delegate>
			{
				readonly static Func<ValueTuple<MethodInfo, MethodDescriptor>, MethodInfo> CreateSelector = Create;

				readonly Type type;

				public Factory( Type type )
				{
					this.type = type;
				}

				public override Delegate Create( MethodDescriptor parameter )
				{
					var method = type.GetRuntimeMethods()
									 .Introduce( parameter, tuple => GenericMethodEqualitySpecification.Default.Get( tuple.Item1 ).IsSatisfiedBy( tuple.Item2 ), CreateSelector )
									 .Introduce( parameter, tuple => CompatibleArgumentsSpecification.Default.Get( tuple.Item1 ).IsSatisfiedBy( tuple.Item2.ArgumentTypes ) )
									 .SingleOrDefault();
					var result = InvokeDelegateFactory.Instance.Get( method );
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

		struct MethodDescriptor
		{
			public MethodDescriptor( string name, Type[] genericTypes, Type[] argumentTypes )
			{
				Name = name;
				GenericTypes = genericTypes;
				ArgumentTypes = argumentTypes;
				Key = new object[] { name, genericTypes, argumentTypes };
			}

			public string Name { get; }
			public Type[] GenericTypes { get; }
			public Type[] ArgumentTypes { get; }
			public object[] Key { get; }
		}
	}

	class ObjectTypeFactory : FactoryBase<object[], Type[]>
	{
		public static ObjectTypeFactory Instance { get; } = new ObjectTypeFactory();

		// [Freeze]
		public override Type[] Create( object[] parameter )
		{
			var result = new Type[parameter.Length];
			for ( var i = 0; i < parameter.Length; i++ )
			{
				result[i] = parameter[i]?.GetType();
			}
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