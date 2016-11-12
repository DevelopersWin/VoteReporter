using JetBrains.Annotations;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace DragonSpark.Specifications
{
	public sealed class ContainsItemSpecification<T> : DelegatedSpecification<T>
	{
		public ContainsItemSpecification( IEnumerable<T> items ) : base( items.Contains ) {}

		[UsedImplicitly]
		public ContainsItemSpecification( ImmutableArray<T> items ) : base( items.Contains ) {}
	}
}