namespace DragonSpark.Activation.Sources.Caching
{
	public interface ICache<T> : ICache<object, T>, IAssignableParameterizedSource<T> {}
}