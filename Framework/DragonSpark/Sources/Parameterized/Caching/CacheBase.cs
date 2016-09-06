namespace DragonSpark.Sources.Parameterized.Caching
{
	public abstract class CacheBase<TInstance, TValue> : AssignableReferenceSourceBase<TInstance, TValue>, ICache<TInstance, TValue>
	{
		public abstract bool Contains( TInstance instance );
		public abstract bool Remove( TInstance instance );
	}
}