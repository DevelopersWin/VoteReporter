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
			new Assignment<T1, T2>( new PropertyAssign<T1, T2>( @this ), Assignments.From( first ), new Value<T2>( second ) );

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

		class Store : BasicFactoryBase<TInstance, IWritableStore<TResult>>
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
			items.Remove( instance );
			items.Add( instance, value );
		}

		public override TValue Get( TInstance instance ) => items.GetValue( instance, create );

		public override bool Remove( TInstance instance ) => items.Remove( instance );
	}

	/*public interface ICompoundCache<T1, T2, TValue> : ICache<ValueTuple<T1, T2>, TValue> {}

	public class CompoundCache<T1, T2, TValue> : CacheBase<ValueTuple<T1, T2>, TValue>, ICompoundCache<T1, T2, TValue> where T2 : class where T1 : class where TValue : class
	{
		readonly static ICache<T1, Cache<T2, TValue>> Default = new ActivatedCache<T1, Cache<T2, TValue>>();
		
		readonly ICache<T1, ICache<T2, TValue>> inner;

		public CompoundCache() : this( Default.Get ) {}
		public CompoundCache( Func<T1, ICache<T2, TValue>> factory ) : this( new Cache<T1, ICache<T2, TValue>>( factory ) ) {}

		public CompoundCache( ICache<T1, ICache<T2, TValue>> inner )
		{
			this.inner = inner;
		}

		public override bool Contains( ValueTuple<T1, T2> instance ) => inner.Get( instance.Item1 ).Contains( instance.Item2 );

		public override bool Remove( ValueTuple<T1, T2> instance ) => inner.Get( instance.Item1 ).Remove( instance.Item2 );

		public override void Set( ValueTuple<T1, T2> instance, TValue value ) => inner.Get( instance.Item1 ).Set( instance.Item2, value );

		public override TValue Get( ValueTuple<T1, T2> instance ) => inner.Get( instance.Item1 ).Get( instance.Item2 );
	}*/

	public abstract class CacheBase<TInstance, TValue> : ICache<TInstance, TValue>
	{
		public abstract bool Contains( TInstance instance );
		public abstract void Set( TInstance instance, TValue value );
		public abstract TValue Get( TInstance instance );
		public abstract bool Remove( TInstance instance );
	}

	public class StoreCache<TValue> : StoreCache<object, TValue>, ICache<TValue>
	{
		/*public StoreCache() {}
		public StoreCache( Func<object, TValue> create ) : this( create, o => new FixedStore<TValue>() ) {}

		public StoreCache( Func<object, IWritableStore<TValue>> store ) : this( new CachedStore<object, TValue>( store ) ) {}
		public StoreCache( Func<object, TValue> create, Func<object, IWritableStore<TValue>> store ) : base( new AssignedAttachedPropertyStore<object, TValue>( create, store ) ) {}

		public StoreCache( CachedStore<object, TValue> create ) : base( create ) {}*/
		public StoreCache() : this( new CacheStore<object, TValue>() ) {}
		public StoreCache( ICache<object, IWritableStore<TValue>> inner ) : base( inner ) {}
	}

	public class StoreCache<TInstance, TValue> : CacheBase<TInstance, TValue> where TInstance : class
	{
		readonly ICache<TInstance, IWritableStore<TValue>> inner;
		// readonly ISubject<AttachedPropertyChangedEvent<TInstance, TValue>> subject = new ReplaySubject<AttachedPropertyChangedEvent<TInstance, TValue>>();

		/*public StoreCache() : this( instance => default(TValue) ) {}

		public StoreCache( Func<TInstance, TValue> create ) : this( new AssignedAttachedPropertyStore<TInstance, TValue>( create ) ) {}

		public StoreCache( Func<TInstance, IWritableStore<TValue>> store ) : this( new CachedStore<TInstance, TValue>( store ) ) {}

		public StoreCache( IAttachedPropertyStore<TInstance, TValue> store ) : this( new ConditionalWeakTable<TInstance, IWritableStore<TValue>>.CreateValueCallback( store.Create ) ) {}

		StoreCache( ConditionalWeakTable<TInstance, IWritableStore<TValue>>.CreateValueCallback create )
		{
			this.create = create;
		}*/

		public StoreCache() : this( new CacheStore<TInstance, TValue>() ) {}

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
		public CacheStore( Func<TInstance, IWritableStore<TValue>> create ) : base( create ) {}
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
	
	
	/*public class ActivatedAttachedPropertyStore<TValue> : ActivatedAttachedPropertyStore<object, TValue> where TValue : new()
	{
		public new static ActivatedAttachedPropertyStore<TValue> Instance { get; } = new ActivatedAttachedPropertyStore<TValue>();
	}

	public class ActivatedAttachedPropertyStore<TInstance, TValue> : AssignedAttachedPropertyStore<TInstance, TValue> where TValue : new() where TInstance : class
	{
		public static ActivatedAttachedPropertyStore<TInstance, TValue> Instance { get; } = new ActivatedAttachedPropertyStore<TInstance, TValue>();

		public ActivatedAttachedPropertyStore() : base( instance => new TValue() ) {}
	}*/

	/*public abstract class ProjectedStore<TInstance, TValue> : ProjectedFactory<TInstance, TValue>
	{
		protected ProjectedStore( Func<TInstance, TValue> convert ) : base( convert ) {}
	}*/



	/*public class AssignedAttachedPropertyStore<TInstance, TValue> : CachedStore<TInstance, TValue> where TInstance : class
	{
		//public new static AssignedAttachedPropertyStore<TInstance, TValue> Instance { get; } = new AssignedAttachedPropertyStore<TInstance, TValue>();

		readonly Func<TInstance, TValue> create;

		protected AssignedAttachedPropertyStore() : this( instance => default(TValue) ) {}

		public AssignedAttachedPropertyStore( Func<TInstance, TValue> create ) : this( create, instance => new FixedStore<TValue>() ) {}

		public AssignedAttachedPropertyStore( Func<TInstance, TValue> create, Func<TInstance, IWritableStore<TValue>> store ) : base( store )
		{
			this.create = create;
		}

		protected virtual TValue CreateValue( TInstance instance ) => create( instance );

		public override IWritableStore<TValue> Create( TInstance instance ) => base.Create( instance ).Assigned( CreateValue( instance ) );
	}*/

	/*public interface IAttachedPropertyStore<in TInstance, TValue> where TInstance : class
	{
		IWritableStore<TValue> Create( TInstance instance );
	}*/

	/*public abstract class AttachedPropertyStoreBase<TInstance, TValue> : IAttachedPropertyStore<TInstance, TValue> where TInstance : class
	{
		public abstract IWritableStore<TValue> Create( TInstance instance );
	}*/

	/*public class CachedStore<TInstance, TValue> : AttachedPropertyStoreBase<TInstance, TValue> where TInstance : class
	{
		readonly Func<TInstance, IWritableStore<TValue>> store;

		public CachedStore( Func<TInstance, IWritableStore<TValue>> store )
		{
			this.store = store;
		}

		public override IWritableStore<TValue> Create( TInstance instance ) => store( instance );
	}*/
}