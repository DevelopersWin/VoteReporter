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

	public abstract class AttachedPropertySpecificationBase<TInstance, TValue> : SpecificationBase<TInstance> where TInstance : class
	{
		protected AttachedPropertySpecificationBase( ICache<TInstance, TValue> property )
		{
			Property = property;
		}

		protected ICache<TInstance, TValue> Property { get; }
	}

	public class AttachedPropertyValueSpecification<TInstance, TValue> : IsAttachedSpecification<TInstance, TValue> where TInstance : class
	{
		readonly Func<TValue> value;

		public AttachedPropertyValueSpecification( ICache<TInstance, TValue> property, Func<TValue> value ) : base( property )
		{
			this.value = value;
		}

		public override bool IsSatisfiedBy( TInstance parameter ) => base.IsSatisfiedBy( parameter ) && Equals( Property.Get( parameter ), value() );
	}

	public class IsAttachedSpecification<TInstance, TValue> : AttachedPropertySpecificationBase<TInstance, TValue> where TInstance : class
	{
		public IsAttachedSpecification( ICache<TInstance, TValue> property ) : base( property ) {}

		public override bool IsSatisfiedBy( TInstance parameter ) => Property.Contains( parameter );
	}

	/*public class DelegatedWritableStore<T> : WritableStore<T>
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
	}*/

	public class DelegatedStore<T> : StoreBase<T>
	{
		readonly Func<T> get;

		public DelegatedStore( Func<T> get )
		{
			this.get = get;
		}

		protected override T Get() => get();
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

	public class DeferredTargetCachedStore<TInstance, TResult> : WritableStore<TResult> where TInstance : class
	{
		readonly Func<TInstance> instance;
		readonly ICache<TInstance, TResult> cache;

		public DeferredTargetCachedStore( Func<TInstance> instance, ICache<TInstance, TResult> cache ) : this( instance, cache, Coercer<TResult>.Instance ) {}
		public DeferredTargetCachedStore( Func<TInstance> instance, ICache<TInstance, TResult> cache, ICoercer<TResult> coercer ) : base( coercer )
		{
			this.instance = instance;
			this.cache = cache;
		}

		protected override TResult Get() => cache.Get( instance() );

		public override void Assign( TResult item ) => cache.Set( instance(), item );
	}

	public class CachedStore<TInstance, TResult> : WritableStore<TResult> where TInstance : class
	{
		readonly TInstance instance;
		readonly ICache<TInstance, TResult> property;

		public CachedStore( TInstance instance, ICache<TInstance, TResult> property ) : this( instance, property, Coercer<TResult>.Instance ) {}
		public CachedStore( TInstance instance, ICache<TInstance, TResult> property, ICoercer<TResult> coercer ) : base( coercer )
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

	public class DecoratedStore<T> : WritableStore<T>
	{
		readonly IWritableStore<T> inner;

		public DecoratedStore( IWritableStore<T> inner )
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
	}
}