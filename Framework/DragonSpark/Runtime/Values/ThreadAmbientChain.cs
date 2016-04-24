using DragonSpark.Extensions;
using PostSharp.Patterns.Contracts;
using System;
using System.Collections.Generic;

namespace DragonSpark.Runtime.Values
{
	public class ThreadAmbientContext : ThreadStoreBase<object>
	{
		public ThreadAmbientContext() : base( typeof(ThreadAmbientContext), () => new object() ) { }

		public static object GetCurrent() => new ThreadAmbientContext().Value;
	}

	public abstract class DecoratedAssociatedStore<T> : DecoratedStore<T>
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
	}

	public abstract class ThreadStoreBase<T> : DecoratedAssociatedStore<T>
	{
		protected ThreadStoreBase( object instance, Func<T> create = null ) : base( instance, () => new ThreadLocalStore<T>( create ) ) {}
	}

	public class ThreadAmbientChain<T> : ThreadStoreBase<Stack<T>>
	{
		public ThreadAmbientChain() : base( ThreadAmbientContext.GetCurrent(), () => new Stack<T>() ) {}

		protected override void OnDispose()
		{
			Value.Clear();
			base.OnDispose();
		}
	}
}