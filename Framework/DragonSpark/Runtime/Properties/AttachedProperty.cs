using DragonSpark.Activation;
using DragonSpark.Runtime.Stores;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace DragonSpark.Runtime.Properties
{
	public static class CacheExtensions
	{
		public static TValue Get<TInstance, TValue>( this TInstance @this, ICache<TInstance, TValue> cache ) where TInstance : class => cache.Get( @this );

		public static TInstance Set<TInstance, TValue>( this TInstance @this, ICache<TInstance, TValue> cache, TValue value ) where TInstance : class
		{
			cache.Set( @this, value );
			return @this;
		}

		public static TValue GetOrSet<TInstance, TValue>( this ICache<TInstance, TValue> @this, TInstance instance, Func<TInstance, TValue> create )
		{
			if ( !@this.Contains( instance ) )
			{
				@this.Set( instance, create( instance ) );
			}
			return @this.Get( instance );
		}

		public static Assignment<T1, T2> Assignment<T1, T2>( this ICache<T1, T2> @this, T1 first, T2 second ) where T1 : class => 
			new Assignment<T1, T2>( new CacheAssign<T1, T2>( @this ), Assignments.From( first ), new Value<T2>( second ) );

		public static Func<TInstance, TValue> ToDelegate<TInstance, TValue>( this ICache<TInstance, TValue> @this ) => DelegateCache<TInstance, TValue>.Default.Get( @this );
		class DelegateCache<TInstance, TValue> : Cache<ICache<TInstance, TValue>, Func<TInstance, TValue>>
		{
			public static DelegateCache<TInstance, TValue> Default { get; } = new DelegateCache<TInstance, TValue>();

			DelegateCache() : base( command => command.Get ) {}
		}

		/*public static TDelegate Apply<TContext, TDelegate>( this ICache<TDelegate, TContext> @this, TDelegate source, TContext context ) where TDelegate : class
		{
			@this.Set( source, context );
			var result = Invocation.Create( source );
			return result;
		}

		public static TContext Context<TContext, TDelegate>( this ICache<TDelegate, TContext> @this ) where TDelegate : class
		{
			var instance = Invocation.GetCurrent() as TDelegate;
			var result = instance != null ? @this.Get( instance ) : default(TContext);
			return result;
		}*/
	}

	public class CacheContext<TInstance, TKey, TResult> where TResult : class where TInstance : class
	{
		readonly Func<TKey, Func<TInstance, TResult>> creator;
		readonly ICache<TInstance, TResult> cache;

		public CacheContext( Func<TKey, Func<TInstance, TResult>> creator ) : this( new Cache<TInstance, TResult>(), creator ) {}

		public CacheContext( ICache<TInstance, TResult> cache, Func<TKey, Func<TInstance, TResult>> creator )
		{
			this.cache = cache;
			this.creator = creator;
		}

		public TResult GetOrSet( TInstance instance, TKey key ) => cache.Contains( instance ) ? cache.Get( instance ) : Set( instance, key );

		TResult Set( TInstance instance, TKey key )
		{
			var result = creator( key )( instance );
			cache.Set( instance, result );
			return result;
		}
	}

	/*public enum AttachedPropertyChangedEventType
	{
		Set, Clear
	}

	public struct AttachedPropertyChangedEvent<TInstance, TValue> where TInstance : class
	{
		public AttachedPropertyChangedEvent( IAttachedProperty<TInstance, TValue> cache, TInstance instance, TValue value = default(TValue), AttachedPropertyChangedEventType type = AttachedPropertyChangedEventType.Clear )
		{
			Property = cache;
			Instance = instance;
			Value = value;
			Type = type;
		}

		public IAttachedProperty<TInstance, TValue> Property { get; }
		public TInstance Instance { get; }
		public TValue Value { get; }
		public AttachedPropertyChangedEventType Type { get; }
	}*/

	/*class StoreConverter<T> : Converter<T, T>
	{
		public StoreConverter( IWritableStore<T> store ) : base( arg => store., @from ) {}
	}*/

	/*class SelfConverter<T> : Converter<T, T>
	{
		public static SelfConverter<T> Instance { get; } = new SelfConverter<T>();

		SelfConverter() : base( Default<T>.Self, Default<T>.Self ) {}
	}

	class TupleConverter<T> : Converter<T, Tuple<T>>
	{
		public static TupleConverter<T> Instance { get; } = new TupleConverter<T>();

		TupleConverter() : base( arg => new Tuple<T>( arg ), tuple => tuple.Item1 ) {}
	}*/

	public class ThreadLocalStoreCache<T> : ThreadLocalStoreCache<object, T>
	{
		public ThreadLocalStoreCache() {}
		public ThreadLocalStoreCache( Func<T> create ) : base( create ) {}

		public ThreadLocalStoreCache( Func<object, IWritableStore<T>> create ) : base( create ) {}
		/*public ThreadLocalStoreCache() : this( () => default(T) ) {}
		public ThreadLocalStoreCache( Func<T> create ) : base( create ) {}

		protected ThreadLocalStoreCache( IAttachedPropertyStore<object, T> store ) : base( store ) {}*/

		// protected ThreadLocalAttachedProperty( Func<object, IWritableStore<T>> store ) : base( store ) {}
	}

	public class ThreadLocalStoreCache<TInstance, TResult> : CacheStore<TInstance, TResult> where TInstance : class
	{
		public ThreadLocalStoreCache() : this( Store.Instance.ToDelegate() ) {}

		public ThreadLocalStoreCache( Func<TResult> create ) : this( new Store( create ).ToDelegate() ) {}

		public ThreadLocalStoreCache( Func<TInstance, IWritableStore<TResult>> create ) : base( create ) {}

		class Store : FactoryBase<TInstance, IWritableStore<TResult>>
		{
			public static Store Instance { get; } = new Store();

			readonly Func<TResult> create;

			Store() : this( () => default(TResult) ) {}

			public Store( Func<TResult> create )
			{
				this.create = create;
			}

			public override IWritableStore<TResult> Create( TInstance instance ) => new ThreadLocalStore<TResult>( create );
		}
	}

	public class SetCache<T> : SetCache<object, T>, ICache<ISet<T>>
	{
		public SetCache() {}
		public SetCache( Func<object, ISet<T>> create ) : base( create ) {}
	}

	public class SetCache<TInstance, TItem> : Cache<TInstance, ISet<TItem>> where TInstance : class
	{
		public SetCache() : base( key => new HashSet<TItem>() ) {}
		public SetCache( Func<TInstance, ISet<TItem>> create ) : base( create ) {}
	}

	public class CollectionCache : CollectionCache<object>
	{
		public new static CollectionCache Default { get; } = new CollectionCache();

		public CollectionCache() {}
		public CollectionCache( Func<object, ICollection<object>> create ) : base( create ) {}
	}

	public class CollectionCache<T> : CollectionCache<object, T>, ICache<ICollection<T>>
	{
		public new static CollectionCache Default { get; } = new CollectionCache();

		public CollectionCache() {}
		public CollectionCache( Func<object, ICollection<T>> create ) : base( create ) {}
	}
	
	public class CollectionCache<TInstance, TItem> : Cache<TInstance, ICollection<TItem>> where TInstance : class
	{
		public static CollectionCache Default { get; } = new CollectionCache();

		public CollectionCache() : base( key => new System.Collections.ObjectModel.Collection<TItem>() ) {}
		public CollectionCache( Func<TInstance, ICollection<TItem>> create ) : base( create ) {}
	}

	/*public abstract class AttachedPropertyBase<TInstance, TValue> : AttachedPropertyBase<TInstance, TValue, TValue> where TInstance : class where TValue : class
	{
		protected AttachedPropertyBase() : this( key => default(TValue) ) {}
		protected AttachedPropertyBase( ConditionalWeakTable<TInstance, TValue>.CreateValueCallback create ) : base( create, SelfConverter<TValue>.Instance ) {}
	}*/

	
	
	public interface ICache<TValue> : ICache<object, TValue> {}
	public interface ICache<in TInstance, TValue>
	{
		bool Contains( TInstance instance );
		
		bool Remove( TInstance instance );

		void Set( TInstance instance, TValue value );

		TValue Get( TInstance instance );
	}

	public class Cache<T> : Cache<object, T>, ICache<T> where T : class
	{
		public Cache() {}
		public Cache( Func<object, T> create ) : base( create ) {}
	}
	

	public class Cache<TInstance, TValue> : CacheBase<TInstance, TValue> where TInstance : class where TValue : class
	{
		readonly ConditionalWeakTable<TInstance, TValue>.CreateValueCallback create;

		readonly ConditionalWeakTable<TInstance, TValue> items = new ConditionalWeakTable<TInstance, TValue>();

		public Cache() : this( new Func<TInstance, TValue>( instance => default(TValue) ) ) {}

		public Cache( Func<TInstance, TValue> create ) : this( new ConditionalWeakTable<TInstance, TValue>.CreateValueCallback( create ) ) {}

		Cache( ConditionalWeakTable<TInstance, TValue>.CreateValueCallback create )
		{
			this.create = create;
		}

		public override bool Contains( TInstance instance )
		{
			TValue temp;
			return items.TryGetValue( instance, out temp );
		}

		public override void Set( TInstance instance, [Optional]TValue value )
		{
			lock ( items )
			{
				items.Remove( instance );
				items.Add( instance, value );
			}
		}

		public override TValue Get( TInstance instance ) => items.GetValue( instance, create );

		public override bool Remove( TInstance instance ) => items.Remove( instance );
	}

	public abstract class CacheBase<TInstance, TValue> : ICache<TInstance, TValue>
	{
		public abstract bool Contains( TInstance instance );
		public abstract void Set( TInstance instance, TValue value );
		public abstract TValue Get( TInstance instance );
		public abstract bool Remove( TInstance instance );
	}

	public class StoreCache<TValue> : StoreCache<object, TValue>, ICache<TValue>
	{
		public StoreCache() : this( new CacheStore<object, TValue>() ) {}
		public StoreCache( ICache<object, IWritableStore<TValue>> inner ) : base( inner ) {}
	}

	public class StoreCache<TInstance, TValue> : CacheBase<TInstance, TValue> where TInstance : class
	{
		readonly ICache<TInstance, IWritableStore<TValue>> inner;

		public StoreCache() : this( instance => default(TValue) ) {}
		public StoreCache( Func<TInstance, TValue> create ) : this( new CacheStore<TInstance, TValue>( create ) ) {}

		public StoreCache( ICache<TInstance, IWritableStore<TValue>> inner )
		{
			this.inner = inner;
		}

		public override bool Contains( TInstance instance ) => inner.Contains( instance );

		public override void Set( TInstance instance, [Optional]TValue value ) => inner.Get( instance ).Assign( value );

		public override TValue Get( TInstance instance ) => inner.Get( instance ).Value;

		public override bool Remove( TInstance instance ) => inner.Remove( instance );
	}

	public class CacheStore<TInstance, TValue> : Cache<TInstance, IWritableStore<TValue>> where TInstance : class
	{
		public CacheStore() : this( instance => new FixedStore<TValue>() ) {}

		public CacheStore( Func<TInstance, TValue> create ) : this( new Func<TInstance, IWritableStore<TValue>>( new Context( create ).Create ) ) {}

		public CacheStore( Func<TInstance, IWritableStore<TValue>> create ) : base( create ) {}

		class Context
		{
			readonly Func<TInstance, TValue> create;
			public Context( Func<TInstance, TValue> create )
			{
				this.create = create;
			}

			public IWritableStore<TValue> Create( TInstance instance ) => new FixedStore<TValue>().Assigned( create( instance ) );
		}
	}


	public class ActivatedCache<T> : ActivatedCache<object, T>, ICache<T> where T : class, new()
	{
		public new static ActivatedCache<T> Instance { get; } = new ActivatedCache<T>();
		public ActivatedCache() {}
	}

	public class ActivatedCache<TInstance, TResult> : Cache<TInstance, TResult> where TInstance : class where TResult : class, new()
	{
		public static ActivatedCache<TInstance, TResult> Instance { get; } = new ActivatedCache<TInstance, TResult>();
		public ActivatedCache() : base( instance => new TResult() ) {}
	}
}