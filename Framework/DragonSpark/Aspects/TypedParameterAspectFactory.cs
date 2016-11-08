using DragonSpark.Activation;
using DragonSpark.Sources.Parameterized;
using DragonSpark.Sources.Parameterized.Caching;
using PostSharp.Aspects;
using System;

namespace DragonSpark.Aspects
{
	public class TypedParameterAspectFactory<TParameter, TResult> : CacheWithImplementedFactoryBase<Type, Func<object, TResult>> where TResult : IAspect
	{
		readonly Func<Type, TParameter> source;

		public TypedParameterAspectFactory( Func<Type, TParameter> source )
		{
			this.source = source;
		}

		protected override Func<object, TResult> Create( Type parameter ) =>
			ParameterConstructor<TParameter, TResult>
				.Default
				.WithParameter( source.WithParameter( parameter ).Get )
				.Wrap();
	}
}