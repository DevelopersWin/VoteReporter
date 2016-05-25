using DragonSpark.Activation;
using System;

namespace DragonSpark.Runtime.Values
{
	public class ExecutionContextStore<T> : WritableStore<T>
	{
		readonly IAttachedProperty<T> property;

		public ExecutionContextStore() : this( () => default(T) ) {}

		public ExecutionContextStore( Func<T> create ) : this( new AttachedProperty<T>( o => create() ) ) {}

		public ExecutionContextStore( IAttachedProperty<T> property )
		{
			this.property = property;
		}

		protected override T Get() => property.Get( Execution.Current );

		public override void Assign( T item ) => property.Set( Execution.Current, item );
	}

	/*public class DecoratedAttachedProperty<T> : IAttachedProperty<T>
	{
		readonly IAttachedProperty<T> inner;

		public DecoratedAttachedProperty( IAttachedProperty<T> inner )
		{
			this.inner = inner;
		}

		public bool IsAttached( object instance ) => inner.IsAttached( instance );

		public void Set( object instance, T value ) => inner.Set( instance, value );

		public T Get( object instance ) => inner.Get( instance );
		public bool Clear( object instance ) => inner.Clear( instance );
	}*/
}