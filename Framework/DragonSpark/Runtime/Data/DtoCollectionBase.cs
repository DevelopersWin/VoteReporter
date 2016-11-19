using DragonSpark.Sources;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;

namespace DragonSpark.Runtime.Data
{
	[DataContract]
	public abstract class DtoCollectionBase<TFrom ,TTo> : Collection<TFrom>, IItemSource<TTo>
	{
		protected DtoCollectionBase() {}
		protected DtoCollectionBase( IList<TFrom> list ) : base( list ) {}

		IEnumerator<TTo> IEnumerable<TTo>.GetEnumerator() => Yield().GetEnumerator();

		protected abstract IEnumerable<TTo> Yield();

		public ImmutableArray<TTo> Get() => Yield().ToImmutableArray();
	}
}