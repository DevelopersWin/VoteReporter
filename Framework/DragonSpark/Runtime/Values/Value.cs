namespace DragonSpark.Runtime.Values
{
	public abstract class ValueBase<T> : IValue<T>
	{
		public T Item => Get();

		// public abstract T Item { get; }

		object IValue.Item => Get();

		protected abstract T Get();
	}

	public class PropertyStore<T> : WritableValue<T>
	{
		public new T Item { get; set; }

		protected override T Get() => Item;

		public override void Assign( T item ) => Item = item;
	}
}