namespace DragonSpark.Runtime.Values
{
	/*public class ThreadAmbientContext : ThreadLocalAttachedProperty<object>
	{
		static ThreadAmbientContext Property { get; } = new ThreadAmbientContext();

		ThreadAmbientContext() : base( () => new object() ) { }

		public static object GetCurrent() => Property.Get( Execution.Current );
	}*/

	/*public abstract class DecoratedAssociatedStore<T> : DecoratedStore<T>
	{
		readonly ConnectedStore<IWritableStore<T>> inner;

		protected DecoratedAssociatedStore( object instance, Func<IWritableStore<T>> create = null ) : this( new AssociatedStore<IWritableStore<T>>( instance, create ) ) { }

		protected DecoratedAssociatedStore( [Required]ConnectedStore<IWritableStore<T>> inner ) : base( inner.Value )
		{
			this.inner = inner;
		}

		protected override void OnDispose()
		{
			inner.TryDispose();
			base.OnDispose();
		}
	}*/

	/*public abstract class ThreadStoreBase<T> : DecoratedAssociatedStore<T>
	{
		protected ThreadStoreBase( object instance, Func<T> create = null ) : base( instance, () => new ThreadLocalStore<T>( create ) ) {}
	}*/
}