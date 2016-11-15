using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace DragonSpark.Sources.Scopes
{
	public class ItemScope<T> : SingletonScope<IEnumerable<T>>, IItemSource<T>
	{
		public ItemScope() {}
		public ItemScope( IEnumerable<T> instance ) : base( instance ) {}
		public ItemScope( Func<IEnumerable<T>> defaultFactory ) : base( defaultFactory ) {}
		public ItemScope( Func<object, IEnumerable<T>> defaultFactory ) : base( defaultFactory ) {}

		ImmutableArray<T> ISource<ImmutableArray<T>>.Get() => Get().ToImmutableArray();
		public IEnumerator<T> GetEnumerator() => Get().GetEnumerator();
		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	}
}