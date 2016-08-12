using DragonSpark.Runtime.Specifications;

namespace DragonSpark.Sources.Caching
{
	public abstract class FactoryCache<T> : FactoryCache<object, T>, ICache<T>
	{
		protected FactoryCache() : this( DefaultSpecification ) {}
		protected FactoryCache( ISpecification<object> specification ) : base( specification ) {}
	}
}