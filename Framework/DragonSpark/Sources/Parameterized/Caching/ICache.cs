namespace DragonSpark.Sources.Parameterized.Caching
{
	public interface ICache<T> : ICache<object, T>, IAssignableParameterizedSource<T> {}
}