using DragonSpark.Sources.Parameterized;
using System;

namespace DragonSpark.Runtime
{
	class InvokeCoercer<TParameter, TResult> : ParameterizedSourceBase<Func<TParameter, TResult>, TResult>
	{
		public override TResult Get( Func<TParameter, TResult> parameter )
		{
			return default(TResult); // TODO
		}
	}
}
