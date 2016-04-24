namespace DragonSpark.Runtime.Values
{
	public interface IValue
	{
		object Item { get; }
	}

	public interface IValue<out T> : IValue
	{
		new T Item { get; }
	}
}