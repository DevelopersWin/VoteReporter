namespace DragonSpark.Sources.Parameterized.Caching
{
	public interface ICache<in TInstance, TValue> : IAssignableParameterizedSource<TInstance, TValue>
	{
		bool Contains( TInstance instance );
		
		bool Remove( TInstance instance );
	}

	public interface ICache<T> : ICache<object, T>, IAssignableParameterizedSource<T> {}
}