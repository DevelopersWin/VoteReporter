using DragonSpark.Activation;
using DragonSpark.Extensions;
using DragonSpark.Runtime.Properties;
using DragonSpark.Runtime.Specifications;
using System;

namespace DragonSpark.Runtime.Stores
{
	public static class StoreExtensions
	{
		public static T Assigned<T, U>( this T @this, U value ) where T : IWritableStore<U>
		{
			@this.Assign( value );
			return @this;
		}
	}

	public class IsAttachedSpecification<TInstance, TValue> : SpecificationBase<TInstance> where TValue : class where TInstance : class
	{
		readonly IAttachedProperty<TInstance, TValue> property;
		public IsAttachedSpecification( IAttachedProperty<TInstance, TValue> property )
		{
			this.property = property;
		}

		public override bool IsSatisfiedBy( TInstance parameter ) => property.IsAttached( parameter );
	}

	public class DelegatedWritableStore<T> : WritableStore<T>
	{
		readonly Func<T> get;
		readonly Action<T> set;
		public DelegatedWritableStore( Func<T> get, Action<T> set ) : this( get, set, Coercer<T>.Instance ) {}
		public DelegatedWritableStore( Func<T> get, Action<T> set, ICoercer<T> coercer ) : base( coercer )
		{
			this.get = get;
			this.set = set;
		}

		protected override T Get() => get();

		public override void Assign( T item ) => set( item );
	}

	public abstract class WritableStore<T> : StoreBase<T>, IWritableStore<T>, IDisposable
	{
		readonly ICoercer<T> coercer;

		protected WritableStore() : this( Coercer<T>.Instance ) {}

		protected WritableStore( ICoercer<T> coercer )
		{
			this.coercer = coercer;
		}

		public abstract void Assign( T item );

		public void Dispose()
		{
			Dispose( true );
			GC.SuppressFinalize( this );
		}

		void Dispose( bool disposing ) => disposing.IsTrue( OnDispose );

		protected virtual void OnDispose() {}

		void IWritableStore.Assign( object item ) => Assign( coercer.Coerce( item ) );
	}

	/*public class ExecutionAssociatedStore<T> : AssociatedStore<T>
	{
		public ExecutionAssociatedStore( object instance, Func<T> create = null ) : base( instance, create ) {}
	}*/

	public class DeferredStore<T> : StoreBase<T>
	{
		readonly Lazy<T> lazy;

		public DeferredStore( Func<T> factory ) : this( new Lazy<T>( factory ) ) {}

		public DeferredStore( Lazy<T> lazy )
		{
			this.lazy = lazy;
		}

		protected override T Get() => lazy.Value;
	}

	public class DeferredAttachedPropertyTargetStore<TInstance, TResult> : WritableStore<TResult> where TInstance : class
	{
		readonly Func<TInstance> instance;
		readonly IAttachedProperty<TInstance, TResult> property;

		public DeferredAttachedPropertyTargetStore( Func<TInstance> instance, IAttachedProperty<TInstance, TResult> property ) : this( instance, property, Coercer<TResult>.Instance ) {}
		public DeferredAttachedPropertyTargetStore( Func<TInstance> instance, IAttachedProperty<TInstance, TResult> property, ICoercer<TResult> coercer ) : base( coercer )
		{
			this.instance = instance;
			this.property = property;
		}

		protected override TResult Get() => property.Get( instance() );

		public override void Assign( TResult item ) => property.Set( instance(), item );
	}

	public class AttachedPropertyStore<TInstance, TResult> : WritableStore<TResult> where TInstance : class
	{
		readonly TInstance instance;
		readonly IAttachedProperty<TInstance, TResult> property;

		public AttachedPropertyStore( TInstance instance, IAttachedProperty<TInstance, TResult> property ) : this( instance, property, Coercer<TResult>.Instance ) {}
		public AttachedPropertyStore( TInstance instance, IAttachedProperty<TInstance, TResult> property, ICoercer<TResult> coercer ) : base( coercer )
		{
			this.instance = instance;
			this.property = property;
		}

		protected override TResult Get() => property.Get( instance );

		public override void Assign( TResult item ) => property.Set( instance, item );
	}

	/*public class DeferredStore<T> : WritableStore<T>
	{
		readonly Func<IWritableStore<T>> deferred;
		
		public DeferredStore( [Required]Func<IWritableStore<T>> deferred )
		{
			this.deferred = deferred;
		}

		public override void Assign( T item ) => deferred.Use( value => value.Assign( item ) );

		protected override T Get() => deferred.Use( value => value.Value );
	}*/

	/*public class DecoratedStore<T> : WritableStore<T>
	{
		readonly IWritableStore<T> inner;

		public DecoratedStore( [Required]IWritableStore<T> inner )
		{
			this.inner = inner;
		}

		public override void Assign( T item ) => inner.Assign( item );

		protected override T Get() => inner.Value;

		protected override void OnDispose()
		{
			inner.TryDispose();
			base.OnDispose();
		}
	}*/
}