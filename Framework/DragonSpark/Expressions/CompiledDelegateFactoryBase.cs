using System;
using System.Linq.Expressions;
using System.Reflection;

namespace DragonSpark.Expressions
{
	public abstract class CompiledDelegateFactoryBase<TParameter, TResult> //: ParameterizedSourceBase<TParameter, TResult>
	{
		readonly static Type Result = typeof(TResult);
		readonly static TypeInfo ResultTypeInfo = Result.GetTypeInfo();
		readonly static Type Void = typeof(void);
		readonly static Type Type1 = ResultTypeInfo.GetDeclaredMethod( nameof(Invoke) ).ReturnType;
		readonly static bool B = Type1 != Void && Type1 != Result;

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
			var converted = B ? Expression.Convert( body, Type1 ) : body;
			var result = Expression.Lambda<TResult>( converted, parameterExpression ).Compile();
			return result;
		}
	}
}