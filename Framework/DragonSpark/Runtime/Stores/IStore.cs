namespace DragonSpark.Runtime.Stores
{
	/*public interface IStoreAware<out T>
	{
		T Value { get; }
	}*/

	public interface IStore
	{
		object Value { get; }
	}

	public interface IStore<out T> : IStore
	{
		new T Value { get; }
	}
}