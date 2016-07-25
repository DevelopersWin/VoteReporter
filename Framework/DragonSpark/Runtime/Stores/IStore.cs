namespace DragonSpark.Runtime.Stores
{
	public interface IStore
	{
		object Value { get; }
	}

	public interface IStore<out T> : IStore
	{
		new T Value { get; }
	}
}