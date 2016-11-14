using DragonSpark.Sources.Parameterized.Caching;
using Xunit;

namespace DragonSpark.Testing.Sources.Parameterized.Caching
{
	public class CacheTests
	{
		[Fact]
		public void Contains()
		{
			var sut = new Cache<object, object>();
			var key = new object();
			Assert.False( sut.Contains( key ) );
			sut.Get( key );
			Assert.True( sut.Contains( key ) );
		}
	}
}