using System.Collections.Generic;

namespace DragonSpark.Runtime
{
	public interface IRepository<T> : IEnumerable<T>, IComposable<T> {}
}