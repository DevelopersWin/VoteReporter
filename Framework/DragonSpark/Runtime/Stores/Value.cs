using DragonSpark.Extensions;
using DragonSpark.TypeSystem;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace DragonSpark.Runtime.Stores
{
	public class Store<T> : StoreBase<T>
	{
		readonly T item;

		public Store( T item )
		{
			this.item = item;
		}

		protected override T Get() => item;
	}

	public abstract class StoreBase<T> : IStore<T>, ISource<T>
	{
		public T Value => Get();

		object IStore.Value => Get();

		protected abstract T Get();

		T ISource<T>.Get() => Get();
	}

	public abstract class ItemsStoreBase<T> : StoreBase<ImmutableArray<T>>
	{
		readonly ImmutableArray<T> items;

		protected ItemsStoreBase( IEnumerable<T> items ) : this( items.Fixed() ) { }

		protected ItemsStoreBase() : this( Items<T>.Default ) {}

		protected ItemsStoreBase( params T[] items )
		{
			this.items = items.ToImmutableArray();
		}

		protected override ImmutableArray<T> Get() => From().ToImmutableArray();

		protected virtual IEnumerable<T> From() => items.ToArray();
	}

	/*public class PropertyStore<T> : WritableStore<T>
	{
		public new T Value { get; set; }

		protected override T Get() => Value;

		public override void Assign( T item ) => Value = item;
	}*/
}