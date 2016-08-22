using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Threading;
using DragonSpark.Sources;

namespace DragonSpark.Runtime
{
	[DebuggerDisplay( "Count = {Count,nq}" ), DebuggerTypeProxy( typeof(ArrayBuilder<A>.DebuggerProxy) )]
	sealed class ArrayBuilder<T> : IReadOnlyList<T>
	{
		#region DebuggerProxy
		sealed class DebuggerProxy
		{
			readonly ArrayBuilder<T> _builder;

			public DebuggerProxy( ArrayBuilder<T> builder )
			{
				_builder = builder;
			}

			[DebuggerBrowsable( DebuggerBrowsableState.RootHidden )]
			public T[] A
			{
				get
				{
					var result = new T[_builder.Count];
					for ( var i = 0; i < result.Length; i++ )
						result[i] = _builder[i];

					return result;
				}
			}
		}
		#endregion

		readonly ImmutableArray<T>.Builder _builder;

		readonly ObjectPool<ArrayBuilder<T>> _pool;

		public ArrayBuilder( int size )
		{
			_builder = ImmutableArray.CreateBuilder<T>( size );
		}

		public ArrayBuilder() : this( 8 ) {}

		ArrayBuilder( ObjectPool<ArrayBuilder<T>> pool ) : this()
		{
			_pool = pool;
		}

		/// <summary>
		///     Realizes the array.
		/// </summary>
		public ImmutableArray<T> ToImmutable() => _builder.ToImmutable();

		public int Count => _builder.Count;

		public T this[ int index ]
		{
			get { return _builder[index]; }
			set { _builder[index] = value; }
		}

		/// <summary>
		///     Write <paramref name="value" /> to slot <paramref name="index" />.
		///     Fills in unallocated slots preceding the <paramref name="index" />, if any.
		/// </summary>
		public void SetItem( int index, T value )
		{
			while ( index > _builder.Count )
				_builder.Add( default(T) );

			if ( index == _builder.Count )
				_builder.Add( value );
			else
				_builder[index] = value;
		}

		public void Add( T item ) => _builder.Add( item );

		public void Insert( int index, T item ) => _builder.Insert( index, item );

		public void EnsureCapacity( int capacity )
		{
			if ( _builder.Capacity < capacity )
				_builder.Capacity = capacity;
		}

		public void Clear() => _builder.Clear();

		public bool Contains( T item ) => _builder.Contains( item );

		public int IndexOf( T item ) => _builder.IndexOf( item );

		public int IndexOf( T item, int startIndex, int count ) => _builder.IndexOf( item, startIndex, count );

		public void RemoveAt( int index ) => _builder.RemoveAt( index );

		public void RemoveLast() => _builder.RemoveAt( _builder.Count - 1 );

		public void ReverseContents() => _builder.Reverse();

		public void Sort() => _builder.Sort();

		public void Sort( IComparer<T> comparer ) => _builder.Sort( comparer );

		public void Sort( int startIndex, IComparer<T> comparer ) => _builder.Sort( startIndex, _builder.Count - startIndex, comparer );

		public T[] ToArray() => _builder.ToArray();

		public void CopyTo( T[] array, int start ) => _builder.CopyTo( array, start );

		public T Last() => _builder[_builder.Count - 1];

		public T First() => _builder[0];

		public bool Any() => _builder.Count > 0;

		/// <summary>
		///     Realizes the array.
		/// </summary>
		public ImmutableArray<T> ToImmutableOrNull() => Count == 0 ? default(ImmutableArray<T>) : ToImmutable();

		/// <summary>
		///     Realizes the array, downcasting each element to a derived type.
		/// </summary>
		public ImmutableArray<U> ToDowncastedImmutable<U>()
			where U : T
		{
			if ( Count == 0 )
				return ImmutableArray<U>.Empty;

			var tmp = ArrayBuilder<U>.GetInstance( Count );
			foreach ( var i in this )
				tmp.Add( (U)i );

			return tmp.ToImmutableAndFree();
		}

		/// <summary>
		///     Realizes the array and disposes the builder in one operation.
		/// </summary>
		public ImmutableArray<T> ToImmutableAndFree()
		{
			var result = ToImmutable();
			Free();
			return result;
		}

		public T[] ToArrayAndFree()
		{
			var result = ToArray();
			Free();
			return result;
		}

		#region Poolable

		// To implement Poolable, you need two things:
		// 1) Expose Freeing primitive. 
		public void Free()
		{
			var pool = _pool;
			if ( pool != null )
				if ( Count < 128 )
				{
					if ( Count != 0 )
						Clear();

					pool.Free( this );
				}
		}

		// 2) Expose the pool or the way to create a pool or the way to get an instance.
		//    for now we will expose both and figure which way works better
		readonly static ObjectPool<ArrayBuilder<T>> Instance = new PoolStore().Get();

		public static ArrayBuilder<T> GetInstance()
		{
			var builder = Instance.Allocate();
			Debug.Assert( builder.Count == 0 );
			return builder;
		}

		public static ArrayBuilder<T> GetInstance( int capacity )
		{
			var builder = GetInstance();
			builder.EnsureCapacity( capacity );
			return builder;
		}

		public static ArrayBuilder<T> GetInstance( int capacity, T fillWithValue )
		{
			var builder = GetInstance();
			builder.EnsureCapacity( capacity );

			for ( var i = 0; i < capacity; i++ )
				builder.Add( fillWithValue );

			return builder;
		}

		public static ObjectPool<ArrayBuilder<T>> CreatePool( int size ) => new PoolStore( size ).Get();

		class PoolStore : FixedSource<ObjectPool<ArrayBuilder<T>>>
		{
			public PoolStore( int size = 128 )
			{
				Assign( new ObjectPool<ArrayBuilder<T>>( Create, size ) );
			}

			ArrayBuilder<T> Create() => new ArrayBuilder<T>( Get() );
		}
		#endregion

		public Enumerator GetEnumerator() => new Enumerator( this );

		IEnumerator<T> IEnumerable<T>.GetEnumerator() => GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

		internal Dictionary<K, ImmutableArray<T>> ToDictionary<K>( Func<T, K> keySelector, IEqualityComparer<K> comparer = null )
		{
			if ( Count == 1 )
			{
				var dictionary1 = new Dictionary<K, ImmutableArray<T>>( 1, comparer );
				var value = this[0];
				dictionary1.Add( keySelector( value ), ImmutableArray.Create( value ) );
				return dictionary1;
			}

			if ( Count == 0 )
				return new Dictionary<K, ImmutableArray<T>>( comparer );

			// bucketize
			// prevent reallocation. it may not have 'count' entries, but it won't have more. 
			var accumulator = new Dictionary<K, ArrayBuilder<T>>( Count, comparer );
			for ( var i = 0; i < Count; i++ )
			{
				var item = this[i];
				var key = keySelector( item );

				ArrayBuilder<T> bucket;
				if ( !accumulator.TryGetValue( key, out bucket ) )
				{
					bucket = GetInstance();
					accumulator.Add( key, bucket );
				}

				bucket.Add( item );
			}

			var dictionary = new Dictionary<K, ImmutableArray<T>>( accumulator.Count, comparer );

			// freeze
			foreach ( var pair in accumulator )
				dictionary.Add( pair.Key, pair.Value.ToImmutableAndFree() );

			return dictionary;
		}

		public void AddRange( ArrayBuilder<T> items )
		{
			_builder.AddRange( items._builder );
		}

		public void AddRange<U>( ArrayBuilder<U> items ) where U : T
		{
			_builder.AddRange( items._builder );
		}

		public void AddRange( ImmutableArray<T> items )
		{
			_builder.AddRange( items );
		}

		public void AddRange( ImmutableArray<T> items, int length )
		{
			_builder.AddRange( items, length );
		}

		public void AddRange( T[] items, int start, int length )
		{
			for ( int i = start, end = start + length; i < end; i++ )
				Add( items[i] );
		}

		public void AddRange( IEnumerable<T> items )
		{
			_builder.AddRange( items );
		}

		public void AddRange( params T[] items )
		{
			_builder.AddRange( items );
		}

		public void AddRange( T[] items, int length )
		{
			_builder.AddRange( items, length );
		}

		public void Clip( int limit )
		{
			Debug.Assert( limit <= Count );
			_builder.Count = limit;
		}

		public void ZeroInit( int count )
		{
			_builder.Clear();
			_builder.Count = count;
		}

		public void AddMany( T item, int count )
		{
			for ( var i = 0; i < count; i++ )
				Add( item );
		}

		public void RemoveDuplicates()
		{
			var set = PooledHashSet<T>.GetInstance();

			var j = 0;
			for ( var i = 0; i < Count; i++ )
				if ( set.Add( this[i] ) )
				{
					this[j] = this[i];
					j++;
				}

			Clip( j );
			set.Free();
		}

		public ImmutableArray<S> SelectDistinct<S>( Func<T, S> selector )
		{
			var result = ArrayBuilder<S>.GetInstance( Count );
			var set = PooledHashSet<S>.GetInstance();

			foreach ( var item in this )
			{
				var selected = selector( item );
				if ( set.Add( selected ) )
					result.Add( selected );
			}

			set.Free();
			return result.ToImmutableAndFree();
		}

		internal struct Enumerator : IEnumerator<T>
		{
			readonly ArrayBuilder<T> _builder;
			int _index;

			public Enumerator( ArrayBuilder<T> builder )
			{
				_builder = builder;
				_index = -1;
			}

			public T Current
			{
				get { return _builder[_index]; }
			}

			public bool MoveNext()
			{
				_index++;
				return _index < _builder.Count;
			}

			public void Dispose() {}

			object IEnumerator.Current
			{
				get { return Current; }
			}

			public void Reset()
			{
				_index = -1;
			}
		}
	}

	public class ObjectPool<T> where T : class
	{
		[DebuggerDisplay( "{Value,nq}" )]
		struct Element
		{
			internal T Value;
		}

		/// <remarks>
		///     Not using System.Func{T} because this file is linked into the (debugger) Formatter,
		///     which does not have that type (since it compiles against .NET 2.0).
		/// </remarks>
		internal delegate T Factory();

		// Storage for the pool objects. The first item is stored in a dedicated field because we
		// expect to be able to satisfy most requests from it.
		T _firstItem;
		readonly Element[] _items;

		// factory is stored for the lifetime of the pool. We will call this only when pool needs to
		// expand. compared to "new T()", Func gives more flexibility to implementers and faster
		// than "new T()".
		readonly Factory _factory;

#if DETECT_LEAKS
		private static readonly ConditionalWeakTable<T, LeakTracker> leakTrackers = new ConditionalWeakTable<T, LeakTracker>();

		private class LeakTracker : IDisposable
		{
			private volatile bool disposed;

#if TRACE_LEAKS
			internal volatile object Trace = null;
#endif

			public void Dispose()
			{
				disposed = true;
				GC.SuppressFinalize(this);
			}

			private string GetTrace()
			{
#if TRACE_LEAKS
				return Trace == null ? "" : Trace.ToString();
#else
				return "Leak tracing information is disabled. Define TRACE_LEAKS on ObjectPool`1.cs to get more info \n";
#endif
			}

			~LeakTracker()
			{
				if (!this.disposed && !Environment.HasShutdownStarted)
				{
					var trace = GetTrace();

					// If you are seeing this message it means that object has been allocated from the pool 
					// and has not been returned back. This is not critical, but turns pool into rather 
					// inefficient kind of "new".
					Debug.WriteLine($"TRACEOBJECTPOOLLEAKS_BEGIN\nPool detected potential leaking of {typeof(T)}. \n Location of the leak: \n {GetTrace()} TRACEOBJECTPOOLLEAKS_END");
				}
			}
		}
#endif

		internal ObjectPool( Factory factory ) : this( factory, Environment.ProcessorCount * 2 ) {}

		internal ObjectPool( Factory factory, int size )
		{
			Debug.Assert( size >= 1 );
			_factory = factory;
			_items = new Element[size - 1];
		}

		T CreateInstance()
		{
			var inst = _factory();
			return inst;
		}

		/// <summary>
		///     Produces an instance.
		/// </summary>
		/// <remarks>
		///     Search strategy is a simple linear probing which is chosen for it cache-friendliness.
		///     Note that Free will try to store recycled objects close to the start thus statistically
		///     reducing how far we will typically search.
		/// </remarks>
		internal T Allocate()
		{
			// PERF: Examine the first element. If that fails, AllocateSlow will look at the remaining elements.
			// Note that the initial read is optimistically not synchronized. That is intentional. 
			// We will interlock only when we have a candidate. in a worst case we may miss some
			// recently returned objects. Not a big deal.
			var inst = _firstItem;
			if ( inst == null || inst != Interlocked.CompareExchange( ref _firstItem, null, inst ) )
				inst = AllocateSlow();

#if DETECT_LEAKS
			var tracker = new LeakTracker();
			leakTrackers.Add(inst, tracker);

#if TRACE_LEAKS
			var frame = CaptureStackTrace();
			tracker.Trace = frame;
#endif
#endif
			return inst;
		}

		T AllocateSlow()
		{
			var items = _items;

			for ( var i = 0; i < items.Length; i++ )
			{
				// Note that the initial read is optimistically not synchronized. That is intentional. 
				// We will interlock only when we have a candidate. in a worst case we may miss some
				// recently returned objects. Not a big deal.
				var inst = items[i].Value;
				if ( inst != null )
					if ( inst == Interlocked.CompareExchange( ref items[i].Value, null, inst ) )
						return inst;
			}

			return CreateInstance();
		}

		/// <summary>
		///     Returns objects to the pool.
		/// </summary>
		/// <remarks>
		///     Search strategy is a simple linear probing which is chosen for it cache-friendliness.
		///     Note that Free will try to store recycled objects close to the start thus statistically
		///     reducing how far we will typically search in Allocate.
		/// </remarks>
		internal void Free( T obj )
		{
			// Validate( obj );
			// ForgetTrackedObject(obj);

			if ( _firstItem == null )
				_firstItem = obj;
			else
				FreeSlow( obj );
		}

		void FreeSlow( T obj )
		{
			var items = _items;
			for ( var i = 0; i < items.Length; i++ )
				if ( items[i].Value == null )
				{
					// Intentionally not using interlocked here. 
					// In a worst case scenario two objects may be stored into same slot.
					// It is very unlikely to happen and will only mean that one of the objects will get collected.
					items[i].Value = obj;
					break;
				}
		}

/*
				[Conditional( "DEBUG" )]
				private void Validate( object obj )
				{
					Debug.Assert( obj != null, "freeing null?" );
		
					Debug.Assert( _firstItem != obj, "freeing twice?" );
		
					var items = _items;
					for ( int i = 0; i < items.Length; i++ )
					{
						var value = items[i].Value;
						if ( value == null )
						{
							return;
						}
		
						Debug.Assert( value != obj, "freeing twice?" );
					}
				}*/
	}

	class PooledHashSet<T> : HashSet<T>
	{
		readonly ObjectPool<PooledHashSet<T>> _pool;

		PooledHashSet( ObjectPool<PooledHashSet<T>> pool )
		{
			_pool = pool;
		}

		public void Free()
		{
			Clear();
			_pool?.Free( this );
		}

		// global pool
		readonly static ObjectPool<PooledHashSet<T>> s_poolInstance = CreatePool();

		// if someone needs to create a pool;
		public static ObjectPool<PooledHashSet<T>> CreatePool()
		{
			ObjectPool<PooledHashSet<T>> pool = null;
			pool = new ObjectPool<PooledHashSet<T>>( () => new PooledHashSet<T>( pool ), 128 );
			return pool;
		}

		public static PooledHashSet<T> GetInstance()
		{
			var instance = s_poolInstance.Allocate();
			Debug.Assert( instance.Count == 0 );
			return instance;
		}
	}
}
