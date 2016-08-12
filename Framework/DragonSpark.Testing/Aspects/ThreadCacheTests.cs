using DragonSpark.Aspects;
using DragonSpark.Extensions;
using DragonSpark.Runtime;
using Ploeh.AutoFixture.Xunit2;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DragonSpark.Sources;
using DragonSpark.Sources.Parameterized.Caching;
using Xunit;

namespace DragonSpark.Testing.Aspects
{
	public class ThreadCacheTests
	{
		static AmbientStack<ThreadCache> Stack { get; } = new AmbientStack<ThreadCache>();

		[Theory, AutoData]
		void SinglePoint( CachedClass sut )
		{
			Assert.Equal( 1, sut.Call() );
			Assert.Equal( 2, sut.Call() );

			var dictionary = new Dictionary<CacheEntry, object>();

			var cache = new ThreadCache( dictionary );

			Assert.Empty( Stack.Get().All().AsEnumerable() );

			using ( new ThreadCacheContext( cache.Self, Stack ) )
			{
				Assert.Equal( 3, sut.Call() );
				Assert.Equal( 3, sut.Call() );

				Task.Run( () =>
				{
					Assert.Equal( 4, sut.Call() );
					Assert.Equal( 5, sut.Call() );

					var inner = new Dictionary<CacheEntry, object>();
					using ( new ThreadCacheContext( new ThreadCache( inner ).Self, Stack ) )
					{
						Assert.Equal( 6, sut.Call() );
						Assert.Equal( 6, sut.Call() );
						Assert.NotEmpty( inner );
					}

					Assert.Empty( inner );

					Assert.Equal( 7, sut.Call() );
					Assert.Equal( 8, sut.Call() );

				} ).Wait();

				Assert.Single( Stack.Get().All().AsEnumerable() );

				using ( new ThreadCacheContext( cache.Self, Stack ) )
				{
					Assert.Equal( 3, sut.Call() );
					Assert.Equal( 3, sut.Call() );
					Assert.Single( Stack.Get().All().AsEnumerable() );
				}

				Assert.Single( Stack.Get().All().AsEnumerable() );

				Assert.Equal( 3, sut.Call() );
				Assert.Equal( 3, sut.Call() );
				Assert.NotEmpty( dictionary );
			}

			Assert.Empty( Stack.Get().All().AsEnumerable() );
			Assert.Empty( dictionary );

			Assert.Equal( 9, sut.Call() );
			Assert.Equal( 10, sut.Call() );
			Assert.Equal( 11, sut.Call() );
		}

		class CachedClass
		{
			int Called { get; set; }

			[ThreadCache]
			public int Call() => ++Called;
		}

		public class ThreadCacheAttribute : DragonSpark.Aspects.ThreadCacheAttribute
		{
			public ThreadCacheAttribute() : base( Stack ) {}
		}
	}
}