namespace DragonSpark.Runtime.Stores
{
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