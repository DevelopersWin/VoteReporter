using Xunit;

namespace DragonSpark.Testing.Runtime
{
	public class CacheTests
	{
		[Fact]
		public void GcCollectionTesting()
		{
			/*var table = new ConditionalWeakTable<object, ReferenceMonitor>();
			table.GetValue( new object(), key => new ReferenceMonitor( table, key ) );*/
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

		
	
	
		/**/

		

		/**/
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
