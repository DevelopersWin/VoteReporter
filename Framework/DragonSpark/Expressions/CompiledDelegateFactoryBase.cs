using System;
using System.Linq.Expressions;
using System.Reflection;

namespace DragonSpark.Expressions
{
	public abstract class CompiledDelegateFactoryBase<TParameter, TResult> //: ParameterizedSourceBase<TParameter, TResult>
	{
		readonly ParameterExpression parameterExpression;
		readonly Func<ExpressionBodyParameter<TParameter>, Expression> bodySource;

		protected CompiledDelegateFactoryBase( Func<ExpressionBodyParameter<TParameter>, Expression> bodySource ) : this( Parameter.Default, bodySource ) {}

		protected CompiledDelegateFactoryBase( ParameterExpression parameterExpression, Func<ExpressionBodyParameter<TParameter>, Expression> bodySource )
		{
			this.parameterExpression = parameterExpression;
			this.bodySource = bodySource;
		}

		public virtual TResult Get( TParameter parameter )
		{
			var body = bodySource( new ExpressionBodyParameter<TParameter>( parameter, parameterExpression ) );
			var type = typeof(TResult).GetTypeInfo().GetDeclaredMethod( nameof(Invoke) ).ReturnType;
			var converted = type != typeof(void) && type != typeof(TResult) ? Expression.Convert( body, type ) : body;
			var result = Expression.Lambda<TResult>( converted, parameterExpression ).Compile();
			return result;
		}
	}
}