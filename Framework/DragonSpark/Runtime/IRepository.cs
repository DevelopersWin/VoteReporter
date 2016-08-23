using System.Collections.Immutable;

namespace DragonSpark.Runtime
{
	public interface IRepository<T> : IComposable<T>
	{
		ImmutableArray<T> List();
	}
}