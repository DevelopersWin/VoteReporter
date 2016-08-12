namespace DragonSpark.Sources
{
	public class FixedSource<T> : AssignableSourceBase<T>
	{
		T reference;

		public FixedSource() {}

		public FixedSource( T reference )
		{
			Assign( reference );
		}

		public sealed override void Assign( T item ) => OnAssign( item );

		protected virtual void OnAssign( T item ) => reference = item;

		public override T Get() => reference;

		protected override void OnDispose() => reference = default(T);
	}
}