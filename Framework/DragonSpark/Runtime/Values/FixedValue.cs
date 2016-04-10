namespace DragonSpark.Runtime.Values
{
	public class FixedValue<T> : WritableValue<T>
	{
		T reference;

		public sealed override void Assign( T item ) => OnAssign( item );

		protected virtual void OnAssign( T item ) => reference = item;

		public override T Item => reference;
	}
}