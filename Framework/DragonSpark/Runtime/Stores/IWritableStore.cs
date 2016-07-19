namespace DragonSpark.Runtime.Stores
{
	public interface IWritableStore<T> : IStore<T>, IWritableStore, IAssignable<T> {}

	public interface IAssignable
	{
		void Assign( object item );
	}

	public interface IAssignable<in T> : IAssignable
	{
		void Assign( T item );
	}

	public interface IWritableStore : IStore, IAssignable {}
}