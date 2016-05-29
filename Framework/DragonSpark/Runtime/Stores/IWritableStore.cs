namespace DragonSpark.Runtime.Stores
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