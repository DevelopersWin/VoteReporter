using DragonSpark.Sources.Parameterized;

namespace DragonSpark.Sources.Caching
{
	public interface ICache<T> : ICache<object, T>, IAssignableParameterizedSource<T> {}
}