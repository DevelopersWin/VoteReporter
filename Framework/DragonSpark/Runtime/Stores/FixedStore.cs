namespace DragonSpark.Runtime.Stores
{
	public class FixedStore<T> : WritableStore<T>
	{
		T reference;

		public FixedStore() {}

		public FixedStore( T reference )
		{
			Assign( reference );
		}

		public sealed override void Assign( T item ) => OnAssign( item );

		protected virtual void OnAssign( T item ) => reference = item;

		protected override T Get() => reference;

		protected override void OnDispose() => reference = default(T);
	}

	// public class DictionaryStore<T>
}