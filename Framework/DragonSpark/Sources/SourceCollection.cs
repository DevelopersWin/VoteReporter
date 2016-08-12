using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using DragonSpark.Runtime;

namespace DragonSpark.Sources
{
	public class SourceCollection<TStore, TInstance> : CollectionBase<TStore> where TStore : ISource<TInstance>
	{
		public SourceCollection() {}
		public SourceCollection( IEnumerable<TStore> items ) : base( items ) {}
		public SourceCollection( ICollection<TStore> source ) : base( source ) {}

		public ImmutableArray<TInstance> Instances() => Query.Select( entry => entry.Get() ).ToImmutableArray();
	}
}