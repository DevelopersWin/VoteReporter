using DragonSpark.Extensions;
using DragonSpark.TypeSystem;
using System.Collections.Generic;
using System.Linq;

namespace DragonSpark.Runtime
{
	public sealed class PurgingCollection<T> : CollectionBase<T>
	{
		public PurgingCollection() : this( Items<T>.Enumerable ) {}
		public PurgingCollection( IEnumerable<T> collection ) : base( collection ) {}
		public PurgingCollection( ICollection<T> source ) : base( source ) {}

		protected override IEnumerable<T> Yield() => Source.Purge().ToArray();
	}
}
