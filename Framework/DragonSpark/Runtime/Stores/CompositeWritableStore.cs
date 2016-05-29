using DragonSpark.Extensions;

namespace DragonSpark.Runtime.Stores
{
	public class CompositeWritableStore<T> : FixedStore<T>
	{
		readonly IWritableStore<T>[] stores;

		public CompositeWritableStore( params IWritableStore<T>[] stores )
		{
			this.stores = stores;
		}

		protected override void OnAssign( T item )
		{
			stores.Each( value => value.Assign( item ) );
			base.OnAssign( item );
		}
	}
}