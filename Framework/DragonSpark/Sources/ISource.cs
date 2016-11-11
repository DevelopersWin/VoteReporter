namespace DragonSpark.Sources
{
	public interface ISource<out T>
	{
		T Get();
	}

	// public interface ISource : ISource<object> {}
}