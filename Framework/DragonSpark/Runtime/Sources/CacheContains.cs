using DragonSpark.Runtime.Sources.Caching;

namespace DragonSpark.Runtime.Sources
{
	public class CacheContains<TInstance, TValue> : CacheSpecificationBase<TInstance, TValue> where TInstance : class
	{
		public CacheContains( ICache<TInstance, TValue> cache ) : base( cache ) {}

		public override bool IsSatisfiedBy( TInstance parameter ) => Cache.Contains( parameter );
	}
}