using DragonSpark.Extensions;
using DragonSpark.TypeSystem;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;

namespace DragonSpark.Runtime.Properties
{
	public class EqualityCache<TKey, TValue> : CacheBase<TKey, TValue>
	{
		public EqualityCache( Func<TKey, TValue> create ) : this( new Dictionary<TKey, TValue>(), create ) {}

		protected EqualityCache( IDictionary<TKey, TValue> store, Func<TKey, TValue> create )
		{
			Store = store;
			Create = create;
		}

		protected IDictionary<TKey, TValue> Store { get; }
		protected Func<TKey, TValue> Create { get; }

		public override bool Contains( TKey instance ) => Store.ContainsKey( instance );

		public override bool Remove( TKey instance ) => Store.Remove( instance );

		public override void Set( TKey instance, TValue value ) => Store[instance] = value;

		public override TValue Get( TKey instance ) => Store.Ensure( instance, Create );
	}

	public class ConcurrentEqualityCache<TKey, TValue> : EqualityCache<TKey, TValue>
	{
		readonly ConcurrentDictionary<TKey, TValue> store;
		public ConcurrentEqualityCache( Func<TKey, TValue> create ) : this( new ConcurrentDictionary<TKey, TValue>(), create ) {}

		ConcurrentEqualityCache( ConcurrentDictionary<TKey, TValue> store, Func<TKey, TValue> create ) : base( store, create )
		{
			this.store = store;
		}

		public override TValue Get( TKey instance ) => store.GetOrAdd( instance, Create );
	}

	// ATTRIBUTION: https://github.com/dotnet/roslyn/blob/master/src/Compilers/Core/Portable/InternalUtilities/WeakList.cs
	sealed class WeakList<T> : IEnumerable<T> where T : class
	{
		const int MinimalNonEmptySize = 4;

		WeakReference<T>[] items = Items<WeakReference<T>>.Default;
		int size;

		void Resize()
		{
			Debug.Assert( size == items.Length );
			Debug.Assert( items.Length == 0 || items.Length >= MinimalNonEmptySize );

			var alive = items.Length;
			var firstDead = -1;
			for ( int i = 0; i < items.Length; i++ )
			{
				T target;
				if ( !items[i].TryGetTarget( out target ) )
				{
					if ( firstDead == -1 )
					{
						firstDead = i;
					}

					alive--;
				}
			}

			if ( alive < items.Length / 4 )
			{
				// If we have just a few items left we shrink the array.
				// We avoid expanding the array until the number of new items added exceeds half of its capacity.
				Shrink( firstDead, alive );
			}
			else if ( alive >= 3 * items.Length / 4 )
			{
				// If we have a lot of items alive we expand the array since just compacting them 
				// wouldn't free up much space (we would end up calling Resize again after adding a few more items).
				var newItems = new WeakReference<T>[GetExpandedSize( items.Length )];

				if ( firstDead >= 0 )
				{
					Compact( firstDead, newItems );
				}
				else
				{
					Array.Copy( items, 0, newItems, 0, items.Length );
					Debug.Assert( size == items.Length );
				}

				items = newItems;
			}
			else
			{
				// Compact in-place to make space for new items at the end.
				// We will free up to length/4 slots in the array.
				Compact( firstDead, items );
			}

			// Debug.Assert(_items.Length > 0 && _size < 3 * _items.Length / 4, "length: " + _items.Length + " size: " + _size);
		}

		void Shrink( int firstDead, int alive )
		{
			var newSize = GetExpandedSize( alive );
			var newItems = ( newSize == items.Length ) ? items : new WeakReference<T>[newSize];
			Compact( firstDead, newItems );
			items = newItems;
		}

		static int GetExpandedSize( int baseSize ) => Math.Max( baseSize * 2 + 1, MinimalNonEmptySize );

		/// <summary>
		/// Copies all live references from <see cref="items"/> to <paramref name="result"/>.
		/// Assumes that all references prior <paramref name="firstDead"/> are alive.
		/// </summary>
		void Compact( int firstDead, WeakReference<T>[] result )
		{
			// Debug.Assert(_items[firstDead].IsNull());

			if ( !ReferenceEquals( items, result ) )
			{
				Array.Copy( items, 0, result, 0, firstDead );
			}

			var oldSize = size;
			var j = firstDead;
			for ( var i = firstDead + 1; i < oldSize; i++ )
			{
				var item = items[i];

				T target;
				if ( item.TryGetTarget( out target ) )
				{
					result[j++] = item;
				}
			}

			size = j;

			// free WeakReferences
			if ( ReferenceEquals( items, result ) )
			{
				while ( j < oldSize )
				{
					items[j++] = null;
				}
			}
		}

		public int WeakCount => size;

		public WeakReference<T> GetWeakReference( int index )
		{
			if ( index < 0 || index >= size )
			{
				throw new ArgumentOutOfRangeException( nameof( index ) );
			}

			return items[index];
		}

		public void Add( T item )
		{
			if ( size == items.Length )
			{
				Resize();
			}

			Debug.Assert( size < items.Length );
			items[size++] = new WeakReference<T>( item );
		}

		public IEnumerator<T> GetEnumerator()
		{
			int count = size;
			int alive = size;
			int firstDead = -1;

			for ( int i = 0; i < count; i++ )
			{
				T item;
				if ( items[i].TryGetTarget( out item ) )
				{
					yield return item;
				}
				else
				{
					// object has been collected 

					if ( firstDead < 0 )
					{
						firstDead = i;
					}

					alive--;
				}
			}

			if ( alive == 0 )
			{
				items = Items<WeakReference<T>>.Default;
				size = 0;
			}
			else if ( alive < items.Length / 4 )
			{
				// If we have just a few items left we shrink the array.
				Shrink( firstDead, alive );
			}
		}

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();

		//  internal WeakReference<T>[] TestOnly_UnderlyingArray { get { return _items; } }
	}

	/*class ReferenceMonitor : IObservable<object>
	{
		readonly object subject;

		readonly Subject<object> inner = new Subject<object>();

		public ReferenceMonitor( object subject )
		{
			this.subject = subject;
		}

		public IDisposable Subscribe( IObserver<object> observer ) => inner.Subscribe( observer );

		~ReferenceMonitor()
		{
			inner.OnNext( subject );
			inner.OnCompleted();
			inner.Dispose();
		}
	}

	class ReferenceMonitorCache : Cache<ReferenceMonitor>
	{
		public static ReferenceMonitorCache Default { get; } = new ReferenceMonitorCache();

		public static Func<object, ReferenceMonitor> Selector { get; } = Default.Get;

		ReferenceMonitorCache() : base( o => new ReferenceMonitor( o ) ) {}
	}

	/*class ReferenceMonitorDictionary<T> : IDictionary<int, T>
	{
		public ReferenceMonitorDictionary( IDictionary<> ) {}
	}#1#
		class WeakCache<T> : Cache<T, WeakReference<T>> where T : class
		{
			public static WeakCache<T> Default { get; } = new WeakCache<T>();

			WeakCache() : base( arg => new WeakReference<T>( arg ) ) {}
		}

		struct Key : IEquatable<Key>
		{
			readonly int code;

			public Key( ImmutableArray<object> subjects )
			{
				code = KeyFactory.Create( subjects );				
			}

			public bool Equals( Key other ) => code == other.code;

			public override bool Equals( object obj ) => !ReferenceEquals( null, obj ) && ( obj is Key && Equals( (Key)obj ) );

			public override int GetHashCode() => code;

			public static bool operator ==( Key left, Key right ) => left.Equals( right );

			public static bool operator !=( Key left, Key right ) => !left.Equals( right );
		}


		class CompoundReferenceCache<T> : ICache<ImmutableArray<object>, T>
		{
			readonly ICache<ICollection<int>> keys = new Cache<ICollection<int>>();

			readonly Action<int> remove;
			readonly Action<object> purge;
			readonly ConcurrentDictionary<int, T> inner = new ConcurrentDictionary<int, T>();

			public CompoundReferenceCache()
			{
				remove = RemoveAction;
				purge = Purge;
			}

			void Purge( object subject ) => keys.Get( subject ).Purge().ForEach( remove );

			void RemoveAction( int key ) => Remove( key );

			bool Remove( int key )
			{
				T result;
				return inner.TryRemove( key, out result );
			}

			public bool Contains( ImmutableArray<object> instance )
			{
				var key = KeyFactory.Create( instance );
				var result = inner.ContainsKey( key );
				return result;
			}

			public bool Remove( ImmutableArray<object> instance )
			{
				var key = KeyFactory.Create( instance );
				return Remove( key );
			}

			public void Set( ImmutableArray<object> instance, T value )
			{
				var key = KeyFactory.Create( instance );
				remove( key );
				instance.Select( ReferenceMonitorCache.Selector ).Amb().Take( 1 ).Subscribe( purge );
				inner.TryAdd( key, value );
			}

			public T Get( ImmutableArray<object> instance )
			{
				var key = KeyFactory.Create( instance );
				T result;
				return inner.TryGetValue( key, out result ) ? result : default(T);
			}
		}*/
}