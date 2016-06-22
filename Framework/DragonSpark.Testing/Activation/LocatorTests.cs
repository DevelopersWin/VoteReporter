using DragonSpark.Activation;
using Ploeh.AutoFixture.Xunit2;
using Xunit;

namespace DragonSpark.Testing.Activation
{
	public class LocatorTests
	{
		[Theory, AutoData]
		void CachesAsExpected( Constructor.ArgumentCache argumentCache )
		{
			var parameter = new ConstructTypeRequest( typeof(ConstructedItem), 6776 );
			var first = argumentCache.Get( parameter );
			Assert.Same( first, argumentCache.Get( parameter ) );
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