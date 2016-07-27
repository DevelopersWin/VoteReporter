using DragonSpark.Activation;
using DragonSpark.Extensions;
using DragonSpark.Runtime.Properties;
using DragonSpark.Runtime.Specifications;
using System;

namespace DragonSpark.Runtime.Stores
{
	public static class StoreExtensions
	{
		public static TValue Assigned<TSource, TValue>( this TSource @this, TValue value ) where TSource : IAssignable<TValue>
		{
			@this.Assign( value );
			return value;
		}

		public static Assignment<T> Assignment<T>( this IWritableStore<T> @this, T first )  => new Assignment<T>( new StoreAssign<T>( @this ), new Value<T>( first ) );
		static class Assign<T>
		{
			public static ICache<IWritableStore<T>, StoreAssign<T>> Cache { get; } = new Cache<IWritableStore<T>, StoreAssign<T>>( c => new StoreAssign<T>( c ) );
		}

		public static T Get<T>( this IStore<T> @this ) => @this.Value;

		/*public static Func<T> ToDelegate<T>( this IStore<T> @this ) where T : class => FixedFactoryCache<T>.Default.Get( @this );
		class FixedFactoryCache<T> : Cache<IStore<T>, Func<T>> where T : class
		{
			public static FixedFactoryCache<T> Default { get; } = new FixedFactoryCache<T>();
			
			FixedFactoryCache() : base( store => new FixedFactory<T>( store.Value ) ) {}
		}*/
	}

	public abstract class CacheSpecificationBase<TInstance, TValue> : SpecificationBase<TInstance> where TInstance : class
	{
		protected CacheSpecificationBase( ICache<TInstance, TValue> cache )
		{
			Cache = cache;
		}

		protected ICache<TInstance, TValue> Cache { get; }
	}

	public class CacheValueSpecification<TInstance, TValue> : CacheContains<TInstance, TValue> where TInstance : class
	{
		readonly Func<TValue> value;

		public CacheValueSpecification( ICache<TInstance, TValue> cache, Func<TValue> value ) : base( cache )
		{
			this.value = value;
		}

		public override bool IsSatisfiedBy( TInstance parameter ) => base.IsSatisfiedBy( parameter ) && Equals( Cache.Get( parameter ), value() );
	}

	public class CacheContains<TInstance, TValue> : CacheSpecificationBase<TInstance, TValue> where TInstance : class
	{
		public CacheContains( ICache<TInstance, TValue> cache ) : base( cache ) {}

		public override bool IsSatisfiedBy( TInstance parameter ) => Cache.Contains( parameter );
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

		void IAssignable.Assign( object item ) => Assign( coercer.Coerce( item ) );
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

	public class DelegatedStore<T> : StoreBase<T>
	{
		readonly Func<T> get;

		public DelegatedStore( Func<T> get )
		{
			this.get = get;
		}

		protected override T Get() => get();
	}

	/*public class DeferredTargetCachedStore<TInstance, TResult> : WritableStore<TResult> where TInstance : class
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
	}*/

	/*public class CachedStore<TInstance, TResult> : WritableStore<TResult> where TInstance : class
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
	}*/

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