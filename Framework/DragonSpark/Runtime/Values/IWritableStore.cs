namespace DragonSpark.Runtime.Values
{
	public interface IWritableStore<T> : IStore<T>, IWritableStore
	{
		void Assign( T item );
	}

	public interface IWritableStore : IStore
	{
		void Assign( object item );
	}
}