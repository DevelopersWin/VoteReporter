using System;

namespace DragonSpark.Sources.Parameterized.Caching
{
	public class ParameterizedSourceCache<TConstructor, TParameter, TResult> : Cache<TConstructor, IParameterizedSource<TParameter, TResult>> where TConstructor : class
	{
		public ParameterizedSourceCache( Func<TConstructor, IParameterizedSource<TParameter, TResult>> create ) : base( create ) {}
	}
}