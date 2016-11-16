using DragonSpark.Sources.Scopes;
using System;

namespace DragonSpark.Sources.Parameterized
{
	public interface IParameterizedScope<TParameter, TResult> : IParameterizedSource<TParameter, TResult>, IScopeAware<Func<TParameter, TResult>> {}
}