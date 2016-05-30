﻿using DragonSpark.Activation;
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

		static AmbientStack<ThreadCache> Stack { get; } = new AmbientStack<ThreadCache>( Context.Self );

		[Theory, AutoData]
		void SinglePoint( CachedClass sut )
		{
			Assert.Equal( 1, sut.Call() );
			Assert.Equal( 2, sut.Call() );

			var dictionary = new Dictionary<CacheEntry, object>();

			var cache = new ThreadCache( dictionary );

			Assert.Empty( Stack.Value.All() );

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

				Assert.Single( Stack.Value.All() );

				using ( new ThreadCacheContext( cache.Self, Stack ) )
				{
					Assert.Equal( 3, sut.Call() );
					Assert.Equal( 3, sut.Call() );
					Assert.Single( Stack.Value.All() );
				}

				Assert.Single( Stack.Value.All() );

				Assert.Equal( 3, sut.Call() );
				Assert.Equal( 3, sut.Call() );
				Assert.NotEmpty( dictionary );
			}

			Assert.Empty( Stack.Value.All() );
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