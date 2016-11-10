﻿using DragonSpark.Sources.Parameterized;

namespace DragonSpark.Aspects.Adapters
{
	public sealed class ParameterizedSourceAdapter<TParameter, TResult> : DelegatedAdapter<TParameter, TResult>, IParameterizedSourceAdapter
	{
		public ParameterizedSourceAdapter( IParameterizedSource<TParameter, TResult> source ) : base( source ) {}
	}
}