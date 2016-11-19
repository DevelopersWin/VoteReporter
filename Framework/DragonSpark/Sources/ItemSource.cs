using DragonSpark.TypeSystem;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DragonSpark.Sources
{
	public class FilteredItemSource<T> : ItemSource<T>
	{
		readonly Func<T, bool> filter;

		public FilteredItemSource( Func<T, bool> filter, IEnumerable<T> items ) : base( items )
		{
			this.filter = filter;
		}

		protected override IEnumerable<T> Yield() => base.Yield().Where( filter );
	}

	public class ItemSource<T> : ItemSourceBase<T>
	{
		readonly IEnumerable<T> items;

		public ItemSource() : this( Items<T>.Default ) {}

		public ItemSource( params T[] items ) : this( items.AsEnumerable() ) {}

		public ItemSource( IEnumerable<T> items )
		{
			this.items = items;
		}

		protected override IEnumerable<T> Yield() => items;
	}
}