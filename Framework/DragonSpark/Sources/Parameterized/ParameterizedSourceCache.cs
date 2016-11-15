using DragonSpark.Sources.Parameterized.Caching;
using System;

namespace DragonSpark.Sources.Parameterized
{
	public class ParameterizedSourceCache<TConstructor, TParameter, TResult> : Cache<TConstructor, IParameterizedSource<TParameter, TResult>> where TConstructor : class
	{
		public ParameterizedSourceCache( Func<TConstructor, IParameterizedSource<TParameter, TResult>> create ) : base( create ) {}
	}
}
