namespace DragonSpark.Runtime.Sources.Caching
{
	public interface ICacheRegistry<T>
	{
		void Register( object key, ICache<T> instance );
		void Clear( object key, object instance );
	}
}