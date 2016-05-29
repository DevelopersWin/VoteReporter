using DragonSpark.Activation;
using DragonSpark.Aspects;
using DragonSpark.Runtime.Properties;
using Ploeh.AutoFixture.Xunit2;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace DragonSpark.Testing.Aspects
{
	public class ThreadCacheTests
	{
		readonly static object Context = new object();

		static AmbientStack<ThreadCache> Cache { get; } = new AmbientStack<ThreadCache>( Context.Self );

		// readonly static AmbientStack<CachedClass> Stack = new AmbientStack<CachedClass>( () => Context, new AmbientStackProperty<CachedClass>() );

		/*class TestStack : AmbientStack<ThreadCache>
		{
			public TestCache( object context, IDictionary<CacheEntry, object> cache ) : base( context.Self, new AmbientStackProperty<ThreadCache>( cache ) ) {}
		}*/

		[Theory, AutoData]
		void SinglePoint( CachedClass sut )
		{
			Assert.Equal( 1, sut.Call() );
			Assert.Equal( 2, sut.Call() );

			var dictionary = new Dictionary<CacheEntry, object>();

			var cache = new ThreadCache( dictionary );

			Assert.Empty( Cache.Value.All() );

			using ( new ThreadCacheContext( cache.Self, Cache ) )
			{
				Assert.Equal( 3, sut.Call() );
				Assert.Equal( 3, sut.Call() );

				Task.Run( () =>
				{
					Assert.Equal( 4, sut.Call() );
					Assert.Equal( 4, sut.Call() );
				} ).Wait();
				Assert.Equal( 4, sut.Call() );
				Assert.Equal( 4, sut.Call() );
				Assert.NotEmpty( dictionary );
			}

			Assert.Empty( Cache.Value.All() );
			Assert.Empty( dictionary );

			Assert.Equal( 3, sut.Call() );
			Assert.Equal( 4, sut.Call() );
			Assert.Equal( 5, sut.Call() );
		}

		class CachedClass
		{
			int Called { get; set; }

			[ThreadCache]
			public int Call() => ++Called;
		}

		public class ThreadCacheAttribute : DragonSpark.Aspects.ThreadCacheAttribute
		{
			public ThreadCacheAttribute() : base( Cache ) {}
		}
	}
}