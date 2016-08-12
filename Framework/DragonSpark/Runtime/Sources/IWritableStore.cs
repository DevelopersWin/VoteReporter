namespace DragonSpark.Runtime.Sources
{
	public interface IWritableStore<T> : IStore<T>, IWritableStore, IAssignable<T> {}

	public interface IWritableStore : IStore, IAssignable {}
}