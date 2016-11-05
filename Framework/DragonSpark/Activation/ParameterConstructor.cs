using DragonSpark.Expressions;
using DragonSpark.Sources.Parameterized;
using DragonSpark.Sources.Parameterized.Caching;
using DragonSpark.TypeSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace DragonSpark.Activation
{
	public class ParameterConstructor<TParameter, TResult> : ParameterizedSourceBase<TParameter, TResult>
	{
		public static Func<TParameter, TResult> Default { get; } = new ParameterConstructor<TParameter, TResult>().Get;
		protected ParameterConstructor() : this( Make() ) {}

		readonly Func<TParameter, TResult> factory;

		ParameterConstructor( Func<TParameter, TResult> factory )
		{
			this.factory = factory;
		}

		public static TResult From( TParameter parameter = default(TParameter) ) => Make( parameter?.GetType() )( parameter );

		public static Func<TParameter, TResult> Make( Type parameterType = null ) => Make( parameterType ?? typeof(TParameter), typeof(TResult) );

		public static Func<TParameter, TResult> Make( Type parameterType, Type resultType )
		{
			var constructor = resultType.Adapt().FindConstructor( parameterType );
			var result = constructor != null ? Make( constructor ) : ( parameter => default(TResult) );
			return result;
		}

		public static Func<TParameter, TResult> Make( ConstructorInfo constructor ) => Factories.Implementation.Get( constructor );

		public override TResult Get( TParameter parameter ) => factory( parameter );

		sealed class Factories : CompiledDelegateFactoryBase<ConstructorInfo, Func<TParameter, TResult>>
		{
			public static IParameterizedSource<ConstructorInfo, Func<TParameter, TResult>> Implementation { get; } = new Cache<ConstructorInfo, Func<TParameter, TResult>>( new Factories().Get );
			Factories() : base( Parameter.Create<TParameter>(), parameter => Expression.New( parameter.Input, CreateParameters( parameter ) ) ) {}

			static IEnumerable<Expression> CreateParameters( ExpressionBodyParameter<ConstructorInfo> parameter )
			{
				var parameters = parameter.Input.GetParameters();
				var type = parameters.First().ParameterType;
				yield return parameter.Parameter.Type == type ? (Expression)parameter.Parameter : Expression.Convert( parameter.Parameter, type );

				foreach ( var source in parameters.Skip( 1 ) )
				{
					yield return Expression.Constant( source.DefaultValue, source.ParameterType );
				}
			}
		}
	}

	public class ParameterConstructor<T> : ParameterConstructor<object, T> {}
}