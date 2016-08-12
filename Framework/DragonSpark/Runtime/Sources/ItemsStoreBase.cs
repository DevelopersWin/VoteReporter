using DragonSpark.Extensions;
using DragonSpark.TypeSystem;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace DragonSpark.Runtime.Sources
{
	public abstract class ItemsStoreBase<T> : SourceBase<ImmutableArray<T>>
	{
		readonly IEnumerable<T> items;
		protected ItemsStoreBase() : this( Items<T>.Default ) {}

		protected ItemsStoreBase( params T[] items ) : this( items.AsEnumerable() ) {}

		protected ItemsStoreBase( IEnumerable<T> items )
		{
			this.items = items;
		}

		public override ImmutableArray<T> Get() => Yield().Prioritize().ToImmutableArray();

		protected virtual IEnumerable<T> Yield() => items;
	}
}