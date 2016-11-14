using DragonSpark.Sources.Parameterized.Caching;
using Xunit;

namespace DragonSpark.Testing.Sources.Parameterized.Caching
{
	public class DictionaryCacheTests
	{
		[Fact]
		public void Set()
		{
			var sut = new ExtendedDictionaryCache<object, object>();
			var key = new object();
			Assert.Null( sut.Get( key ) );
			var o = new object();
			sut.Set( key, o );
			Assert.Same( o, sut.Get( key ) );
			sut.Remove( key );
			Assert.Null( sut.Get( key ) );
		}

		[Fact]
		public void GetOrSet()
		{
			var sut = new ExtendedDictionaryCache<object, Item>( o => new Item( 1001 ) );
			var key = new object();
			var source = sut.Get( key );
			Assert.Equal( 1001, source.Id );
		}

		[Fact]
		public void GetOrSetCreated()
		{
			var sut = new ExtendedDictionaryCache<object, Item>();
			var key = new object();
			
			Assert.Null( sut.Get( key ) );
		}

		sealed class Item
		{
			public Item( int id )
			{
				Id = id;
			}

			public int Id { get; }
		}
	}
}