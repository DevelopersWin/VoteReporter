using System.Collections.Generic;

namespace DragonSpark.Sources.Scopes
{
	public class ItemScope<T> : DecoratedItemSource<T>
	{
		public ItemScope( IScope<IEnumerable<T>> scope ) : base( scope )
		{
			Scope = scope;
		}

		public IScope<IEnumerable<T>> Scope { get; }
	}
}