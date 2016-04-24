namespace DragonSpark.Runtime.Values
{
	public class FixedStore<T> : WritableStore<T>
	{
		T reference;

		public sealed override void Assign( T item ) => OnAssign( item );

		protected virtual void OnAssign( T item ) => reference = item;

		protected override T Get() => reference;
	}
}