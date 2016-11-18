using System.Collections.Generic;

namespace DragonSpark.Runtime
{
	public abstract class RepositoryBase<T> : CollectionBase<T>, IRepository<T>
	{
		protected RepositoryBase() : this( new List<T>() ) {}

		protected RepositoryBase( IEnumerable<T> items ) : this( new List<T>( items ) ) {}

		protected RepositoryBase( ICollection<T> source ) : base( source ) {}

		// protected override IEnumerable<T> Yield() => Source;

		// public ImmutableArray<T> Get() => Yield().ToImmutableArray();
	}
}