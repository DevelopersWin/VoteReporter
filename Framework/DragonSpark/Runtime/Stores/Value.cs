namespace DragonSpark.Runtime.Stores
{
	/*public class Store<T> : StoreBase<T>
	{
		readonly T item;

		public Store( T item )
		{
			this.item = item;
		}

		protected override T Get() => item;
	}*/

	public abstract class StoreBase<T> : IStore<T>
	{
		public T Value => Get();

		object IStore.Value => Get();

		protected abstract T Get();
	}

	public class PropertyStore<T> : WritableStore<T>
	{
		public new T Value { get; set; }

		protected override T Get() => Value;

		public override void Assign( T item ) => Value = item;
	}
}