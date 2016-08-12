using DragonSpark.Aspects.Validation;
using DragonSpark.Extensions;
using DragonSpark.Runtime;
using DragonSpark.Runtime.Specifications;
using DragonSpark.TypeSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using DragonSpark.Activation.Sources.Caching;

namespace DragonSpark.Activation
{
	[ApplyAutoValidation]
	public class Constructor : ConstructorBase
	{
		public static Constructor Instance { get; } = new Constructor();
		Constructor() : this( ConstructorStore.Instance.Get, ConstructorDelegateFactory<Invoke>.Default.Get ) {}

		readonly Func<ConstructTypeRequest, ConstructorInfo> constructorSource;
		readonly Func<ConstructorInfo, Invoke> activatorSource;

		Constructor( Func<ConstructTypeRequest, ConstructorInfo> constructorSource, Func<ConstructorInfo, Invoke> activatorSource ) : base( Specification.Instance )
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

	public class ConstructorStore : EqualityReferenceCache<ConstructTypeRequest, ConstructorInfo>
	{
		public static ConstructorStore Instance { get; } = new ConstructorStore();

		ConstructorStore() : base( Create ) {}

		static ConstructorInfo Create( ConstructTypeRequest parameter )
		{
			var types = ObjectTypeFactory.Instance.Create( parameter.Arguments );
			var candidates = new [] { types, types.WhereAssigned().Fixed(), Items<Type>.Default };
			var adapter = parameter.RequestedType.Adapt();
			var result = candidates.Distinct( StructuralEqualityComparer<Type[]>.Instance )
				.Introduce( adapter , tuple => tuple.Item2.FindConstructor( tuple.Item1 )  )
				.FirstOrDefault();
			return result;
		}
	}

	class InvokeMethodDelegate<T> : InvocationFactoryBase<MethodInfo, T> where T : class
	{
		public static ICache<MethodInfo, T> Instance { get; } = new Cache<MethodInfo, T>( new InvokeMethodDelegate<T>().Create );
		InvokeMethodDelegate() : base( InvokeMethodExpressionFactory.Instance.Create ) {}
	}

	class InvokeInstanceMethodDelegate<T> : InvocationFactoryBase<MethodInfo, T> where T : class
	{
		public InvokeInstanceMethodDelegate( object instance ) : base( new InvokeInstanceMethodExpressionFactory( instance ).Create ) {}
	}

	class ConstructorDelegateFactory<T> :  InvocationFactoryBase<ConstructorInfo, T> where T : class
	{
		public static ICache<ConstructorInfo, T> Default { get; } = new Cache<ConstructorInfo, T>( new ConstructorDelegateFactory<T>().Create );
		ConstructorDelegateFactory() : base( ActivateFromArrayExpression.Instance.Create ) {}
	}

	abstract class InvocationFactoryBase<TParameter, TDelegate> : CompiledDelegateFactoryBase<TParameter, TDelegate> where TParameter : MethodBase
	{
		protected InvocationFactoryBase( Func<ExpressionBodyParameter<TParameter>, Expression> bodySource ) : this( Parameter.Default, bodySource ) {}
		protected InvocationFactoryBase( ParameterExpression expression, Func<ExpressionBodyParameter<TParameter>, Expression> bodySource ) : base( expression, bodySource ) {}
	}

	class ActivateFromArrayExpression : InvokeArrayFactoryBase<ConstructorInfo>
	{
		public static ActivateFromArrayExpression Instance { get; } = new ActivateFromArrayExpression();
		ActivateFromArrayExpression() {}

		protected override Expression Apply( ExpressionBodyParameter<ConstructorInfo> parameter, Expression[] arguments ) => Expression.New( parameter.Input, arguments );
	}

	class InvokeMethodExpressionFactory : InvokeArrayFactoryBase<MethodInfo>
	{
		public static InvokeMethodExpressionFactory Instance { get; } = new InvokeMethodExpressionFactory();
		InvokeMethodExpressionFactory() {}

		protected override Expression Apply( ExpressionBodyParameter<MethodInfo> parameter, Expression[] arguments ) => Expression.Call( parameter.Input, arguments );
	}

	class InvokeInstanceMethodExpressionFactory : InvokeArrayFactoryBase<MethodInfo>
	{
		readonly object instance;
		public InvokeInstanceMethodExpressionFactory( object instance )
		{
			this.instance = instance;
		}

		protected override Expression Apply( ExpressionBodyParameter<MethodInfo> parameter, Expression[] arguments ) => Expression.Call( Expression.Constant( instance ), parameter.Input, arguments );
	}

	abstract class InvokeArrayFactoryBase<T> /*: FactoryBase<ExpressionBodyParameter<T>, Expression>*/ where T : MethodBase
	{
		public virtual Expression Create( ExpressionBodyParameter<T> parameter )
		{
			var array = ArgumentsArrayExpressionFactory.Instance.Create( new ArgumentsArrayParameter( parameter.Input, parameter.Parameter ) );
			var result = Apply( parameter, array );
			return result;
		}

		protected abstract Expression Apply( ExpressionBodyParameter<T> parameter, Expression[] arguments );
	}

	class ArgumentsArrayExpressionFactory : FactoryBase<ArgumentsArrayParameter, Expression[]>
	{
		public static ArgumentsArrayExpressionFactory Instance { get; } = new ArgumentsArrayExpressionFactory();
		ArgumentsArrayExpressionFactory() {}

		public override Expression[] Create( ArgumentsArrayParameter parameter )
		{
			var types = parameter.Method.GetParameterTypes();
			var result = new Expression[types.Length];
			for ( var i = 0; i < types.Length; i++ )
			{
				var index = Expression.ArrayIndex( parameter.Parameter, Expression.Constant( i ) );
				result[i] = Expression.Convert( index, types[i] );
			}
			return result;
		}
	}

	class ArgumentsArrayParameter
	{
		public ArgumentsArrayParameter( MethodBase method, ParameterExpression parameter )
		{
			Method = method;
			Parameter = parameter;
		}

		public MethodBase Method { get; }
		public ParameterExpression Parameter { get; }
	}

	public struct ExpressionBodyParameter<T>
	{
		public ExpressionBodyParameter( T input, ParameterExpression parameter )
		{
			Input = input;
			Parameter = parameter;
		}

		public T Input { get; }
		public ParameterExpression Parameter { get; }
	}

	/*public interface IParameterExpressionStore : IStore<ParameterExpression> {}*/

	public static class Parameter
	{
		public static ParameterExpression Create<T>( string name = "parameter" ) => Expression.Parameter( typeof(T), name );

		public static ParameterExpression Default { get; } = Create<object[]>();

		/*public static ImmutableArray<ParameterExpression> InstanceArguments { get; } = new ParameterFactory( InstanceParameter.Instance, ArgumentArrayParameter.Instance ).Create();*/
		// public static ImmutableArray<ParameterExpression> Arguments { get; } = new ParameterFactory( ArgumentArrayParameter.Instance ).Create();
	}

	/*public class ParameterFactory : FactoryBase<ImmutableArray<ParameterExpression>>
	{
		readonly IParameterExpressionStore[] stores;

		public ParameterFactory( params IParameterExpressionStore[] stores )
		{
			this.stores = stores;
		}

		public override ImmutableArray<ParameterExpression> Create() => stores.Select( store => store.Value ).ToImmutableArray();
	}*/

	/*public class InstanceParameter : InstanceParameter<object>
	{
		public new static InstanceParameter Instance { get; } = new InstanceParameter();
		InstanceParameter() {}
	}

	public class InstanceParameter<T> : ExpressionParameterStoreBase<T>
	{
		public static InstanceParameter<T> Instance { get; } = new InstanceParameter<T>();
		protected InstanceParameter() : base( "instance" ) {}
	}*/

	/*public class ArgumentArrayParameter : ExpressionParameterStoreBase<object[]>
	{
		public static ArgumentArrayParameter Instance { get; } = new ArgumentArrayParameter();
		ArgumentArrayParameter() : base( "arguments" ) {}
	}

	public abstract class ExpressionParameterStoreBase<T> : StoreBase<ParameterExpression>, IParameterExpressionStore
	{
		readonly ParameterExpression expression;

		protected ExpressionParameterStoreBase( string name ) : this( Expression.Parameter( typeof(T), name ) ) {}

		protected ExpressionParameterStoreBase( ParameterExpression expression )
		{
			this.expression = expression;
		}

		protected override ParameterExpression Get() => expression;
	}*/

	public abstract class CompiledDelegateFactoryBase<TParameter, TResult> // : FactoryBase<TParameter, TResult>
	{
		readonly ParameterExpression parameterExpression;
		readonly Func<ExpressionBodyParameter<TParameter>, Expression> bodySource;

		protected CompiledDelegateFactoryBase( Func<ExpressionBodyParameter<TParameter>, Expression> bodySource ) : this( Parameter.Default, bodySource ) {}

		protected CompiledDelegateFactoryBase( ParameterExpression parameterExpression, Func<ExpressionBodyParameter<TParameter>, Expression> bodySource )
		{
			this.parameterExpression = parameterExpression;
			this.bodySource = bodySource;
		}

		public virtual TResult Create( TParameter parameter )
		{
			var body = bodySource( new ExpressionBodyParameter<TParameter>( parameter, parameterExpression ) );
			var type = typeof(TResult).GetTypeInfo().GetDeclaredMethod( nameof(Invoke) ).ReturnType;
			var converted = type != typeof(void) && type != typeof(TResult) ? Expression.Convert( body, type ) : body;
			var result = Expression.Lambda<TResult>( converted, parameterExpression ).Compile();
			return result;
		}
	}

	/*public static class InvokeExtensions
	{
		public static T Invoke<T>( this Invoke @this, params object[] arguments ) => (T)@this( arguments );
	}*/

	public delegate object Invoke( params object[] args );

	public delegate void Execute( params object[] args );
}