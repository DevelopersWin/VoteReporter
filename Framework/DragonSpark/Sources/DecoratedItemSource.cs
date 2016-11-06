using System.Collections.Generic;

namespace DragonSpark.Sources
{
	public class DecoratedItemSource<T> : DelegatedItemSource<T>
	{
		public DecoratedItemSource( ISource<IEnumerable<T>> scope ) : base( scope.Get ) {}
	}
}