using System.Collections.Generic;
using System.Collections.Immutable;

namespace DragonSpark.Runtime
{
	public abstract class RepositoryBase<T> : IRepository<T>
	{
		protected RepositoryBase() : this( new List<T>() ) {}

		protected RepositoryBase( IEnumerable<T> items ) : this( new List<T>( items ) ) {}

		protected RepositoryBase( ICollection<T> source )
		{
			Source = source;
		}

		protected ICollection<T> Source { get; }

		public void Add( T instance ) => OnAdd( instance );

		protected virtual void OnAdd( T entry ) => Source.Add( entry );

		public virtual ImmutableArray<T> List() => Query().ToImmutableArray();

		protected virtual IEnumerable<T> Query() => Source;
	}
}