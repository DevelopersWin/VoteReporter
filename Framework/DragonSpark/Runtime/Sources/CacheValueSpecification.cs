using System;
using DragonSpark.Runtime.Sources.Caching;

namespace DragonSpark.Runtime.Sources
{
	public class CacheValueSpecification<TInstance, TValue> : CacheContains<TInstance, TValue> where TInstance : class
	{
		readonly Func<TValue> value;

		public CacheValueSpecification( ICache<TInstance, TValue> cache, Func<TValue> value ) : base( cache )
		{
			this.value = value;
		}

		public override bool IsSatisfiedBy( TInstance parameter ) => base.IsSatisfiedBy( parameter ) && Equals( Cache.Get( parameter ), value() );
	}
}