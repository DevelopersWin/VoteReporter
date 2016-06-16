using Xunit;

namespace DragonSpark.Testing.Runtime
{
	public class CacheTests
	{
		[Fact]
		public void GcCollectionTesting()
		{
			/*var table = new ConditionalWeakTable<object, Monitor>();
			table.GetValue( new object(), key => new Monitor( table, key ) );*/
			/*GC.Collect();
			GC.WaitForPendingFinalizers();
			Debugger.Break();*/
		}

	/*	[Fact]
		public void TupleKey()
		{
			var table = new ConditionalWeakTable<object, object>();
			var key = new object();
			table.Add( key, new object() );
	
			// Debugger.Break(); // Table has one entry here.

			// TypedReference tr = __makeref( key );

			GC.Collect();
			GC.WaitForPendingFinalizers();
	
			Debugger.Break(); // Table is empty here.
				
		}*/

		[Fact]
		public void Testing()
		{
			/*var instance = new Factory();
			CreateProfilerEvent profiler = instance.Get;*/
		}

		
	
	
		/*class MonitorCache : Cache<Monitor>
		{
			public static MonitorCache Default { get; } = new MonitorCache();

			public static Func<object, Monitor> Selector { get; } = Default.Get;

			MonitorCache() : base( o => new Monitor( o ) ) {}
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
				var key = KeyFactory.Instance.Create( instance );
				var result = inner.ContainsKey( key );
				return result;
			}

			public bool Remove( ImmutableArray<object> instance )
			{
				var key = KeyFactory.Instance.Create( instance );
				return Remove( key );
			}

			public void Set( ImmutableArray<object> instance, T value )
			{
				var key = KeyFactory.Instance.Create( instance );
				remove( key );
				instance.Select( MonitorCache.Selector ).Amb().Take( 1 ).Subscribe( purge );
				inner.TryAdd( key, value );
			}

			public T Get( ImmutableArray<object> instance )
			{
				var key = KeyFactory.Instance.Create( instance );
				T result;
				return inner.TryGetValue( key, out result ) ? result : default(T);
			}
		}*/

		/*class WeakCache<T> : Cache<T, WeakReference<T>> where T : class
		{
			public static WeakCache<T> Default { get; } = new WeakCache<T>();

			WeakCache() : base( arg => new WeakReference<T>( arg ) ) {}
		}*/

		/*struct Key : IEquatable<Key>
		{
			readonly int code;

			public Key( ImmutableArray<object> subjects )
			{
				code = KeyFactory.Instance.Create( subjects );				
			}

			public bool Equals( Key other ) => code == other.code;

			public override bool Equals( object obj ) => !ReferenceEquals( null, obj ) && ( obj is Key && Equals( (Key)obj ) );

			public override int GetHashCode() => code;

			public static bool operator ==( Key left, Key right ) => left.Equals( right );

			public static bool operator !=( Key left, Key right ) => !left.Equals( right );
		}*/

		/*class Monitor : IObservable<object>
		{
			readonly object subject;

			readonly Subject<object> inner = new Subject<object>();

			public Monitor( object subject )
			{
				this.subject = subject;
			}

			public IDisposable Subscribe( IObserver<object> observer ) => inner.Subscribe( observer );

			~Monitor()
			{
				inner.OnNext( subject );
				inner.OnCompleted();
				inner.Dispose();
			}
		}*/
	}

	/*public static class FunctionalEx
	{
		public static Func<T1, Func<T2, TResult>> Curry<T1, T2, TResult>(this Func<T1, T2, TResult> fn) => 
			Implementation<T1, T2, TResult>.Curry(fn);

		public static Func<T2, T1, TResult> Flip<T1, T2, TResult>(this Func<T1, T2, TResult> fn) => 
			Implementation<T1, T2, TResult>.Flip(fn);

		static class Implementation<X, Y, Z>
		{
			public static Func<Func<X, Y, Z>, Func<X, Func<Y, Z>>> Curry { get; } = fn => x => y => fn( x, y );

			public static Func<Func<X, Y, Z>, Func<Y, X, Z>> Flip { get; } = fn => ( y, x ) => fn( x, y );
		}
	}*/
}
