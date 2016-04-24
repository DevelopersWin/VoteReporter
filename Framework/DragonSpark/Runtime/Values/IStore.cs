namespace DragonSpark.Runtime.Values
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