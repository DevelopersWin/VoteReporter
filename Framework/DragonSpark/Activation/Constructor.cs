using DragonSpark.Aspects;
using DragonSpark.Extensions;
using DragonSpark.Runtime;
using DragonSpark.Runtime.Properties;
using DragonSpark.Runtime.Specifications;
using DragonSpark.TypeSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace DragonSpark.Activation
{
	[AutoValidation.GenericFactory]
	public class Constructor : ConstructorBase
	{
		readonly Func<ConstructTypeRequest, ConstructorInfo> constructorSource;
		readonly Func<ConstructorInfo, Invocation> activatorSource;

		public static Constructor Instance { get; } = new Constructor();

		Constructor() : this( ConstructorStore.Instance.ToDelegate(), InvocationStore.Instance.ToDelegate() ) {}

		Constructor( Func<ConstructTypeRequest, ConstructorInfo> constructorSource, Func<ConstructorInfo, Invocation> activatorSource ) : base( Specification.Instance )
		{
			this.constructorSource = constructorSource;
			this.activatorSource = activatorSource;
		}

		public T Create<T>( ConstructTypeRequest parameter ) => (T)Create( parameter );

		public override object Create( ConstructTypeRequest parameter ) => LocateAndCreate( parameter ) ?? SpecialValues.DefaultOrEmpty( parameter.RequestedType );

		object LocateAndCreate( ConstructTypeRequest parameter )
		{
			var info = constructorSource( parameter );
			var result = info != null ? activatorSource( info )?.Invoke( WithOptional( parameter.Arguments, info.GetParameters() ) ) : null;
			return result;
		}

		static object[] WithOptional( IReadOnlyCollection<object> arguments, IEnumerable<ParameterInfo> parameters )
		{
			var optional = parameters.Skip( arguments.Count ).Where( info => info.IsOptional ).Select( info => info.DefaultValue );
			var result = arguments.Concat( optional ).Fixed();
			return result;
		}

		class Specification : GuardedSpecificationBase<ConstructTypeRequest>
		{
			public static Specification Instance { get; } = new Specification();

			readonly ConstructorStore cache;

			Specification() : this( ConstructorStore.Instance ) {}
			Specification( ConstructorStore cache ) : base( Coercer.Instance.ToDelegate() )
			{
				this.cache = cache;
			}

			public override bool IsSatisfiedBy( ConstructTypeRequest parameter ) => parameter.RequestedType.GetTypeInfo().IsValueType || cache.Get( parameter ) != null;
		}
	}

	public class ConstructorStore : EqualityCache<ConstructTypeRequest, ConstructorInfo>
	{
		public static ConstructorStore Instance { get; } = new ConstructorStore();

		ConstructorStore() : base( Create ) {}

		static ConstructorInfo Create( ConstructTypeRequest parameter )
		{
			var types = ObjectTypeFactory.Instance.Create( parameter.Arguments );
			var candidates = new [] { types, types.WhereAssigned().Fixed(), Items<Type>.Default };
			var adapter = parameter.RequestedType.Adapt();
			var result = candidates.Distinct( StructuralEqualityComparer<Type[]>.Instance )
				.Introduce( adapter )
				.Select( tuple => tuple.Item2.FindConstructor( tuple.Item1 )  )
				.FirstOrDefault();
			return result;
		}
	}

	class InvocationStore : Cache<ConstructorInfo, Invocation>
	{
		public static InvocationStore Instance { get; } = new InvocationStore();
		InvocationStore() : base( ConstructorDelegateFactory<Invocation>.Instance.ToDelegate() ) {}
	}

	class InvocationFactory : InvocationFactoryBase<MethodInfo>
	{
		public static InvocationFactory Instance { get; } = new InvocationFactory();
		InvocationFactory() : base( Expression.Call ) {}
	}

	class ConstructorDelegateFactory<T> :  InvocationFactoryBase<ConstructorInfo>
	{
		public static ConstructorDelegateFactory<T> Instance { get; } = new ConstructorDelegateFactory<T>();
		ConstructorDelegateFactory() : base( Expression.New ) {}
	}

	abstract class InvocationFactoryBase<T> : CompiledDelegateFactoryBase<T, object[], Invocation> where T : MethodBase
	{
		readonly Func<T, Expression[], Expression> factory;

		protected InvocationFactoryBase( Func<T, Expression[], Expression> factory )
		{
			this.factory = factory;
		}

		protected override Expression CreateBody( T parameter, ParameterExpression definition )
		{
			var types = parameter.GetParameterTypes();

			var array = new Expression[types.Length];
			for ( var i = 0; i < types.Length; i++ )
			{
				var index = Expression.ArrayIndex( definition, Expression.Constant( i ) );
				array[i] = Expression.Convert( index, types[i] );
			}

			var result = factory( parameter, array );
			return result;
		}
	}

	abstract class CompiledDelegateFactoryBase<TSource, TParameter, TResult> : FactoryBase<TSource, TResult>
	{
		public override TResult Create( TSource parameter )
		{
			var definition = Expression.Parameter( typeof(TParameter), nameof(parameter) );
			var body = CreateBody( parameter, definition );
			var result = Expression.Lambda<TResult>( body, definition ).Compile();
			return result;
		}

		protected abstract Expression CreateBody( TSource parameter, ParameterExpression definition );
	}

	delegate object Invocation( params object[] args );
}