using DragonSpark.Activation;
using Ploeh.AutoFixture.Xunit2;
using Xunit;

namespace DragonSpark.Testing.Activation
{
	public class TypeLocatorCacheBaseryTests
	{
		[Theory, AutoData]
		void CachesAsExpected( Constructor.Locator locator )
		{
			var parameter = new ConstructTypeRequest( typeof(ConstructedItem), 6776 );
			var first = locator.Create( parameter );
			Assert.Same( first, locator.Create( parameter ) );
		}

		class ConstructedItem
		{
			public ConstructedItem( int number )
			{
				Number = number;
			}

			public int Number { get; }
		}
	}
}