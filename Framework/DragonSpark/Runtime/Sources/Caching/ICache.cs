namespace DragonSpark.Runtime.Sources.Caching
{
	public interface ICache<T> : ICache<object, T>, IAssignableParameterizedSource<T> {}
}