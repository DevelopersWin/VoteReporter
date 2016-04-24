namespace DragonSpark.Runtime.Values
{
	public interface IWritableStore<T> : IStore<T>
	{
		void Assign( T item );
	}
}