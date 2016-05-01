using DragonSpark.Activation;
using Ploeh.AutoFixture.Xunit2;
using Xunit;

namespace DragonSpark.Testing.Activation
{
	public class ConstructionActivatorFactoryTests
	{
		[Theory, AutoData]
		void CachesAsExpected( Constructor.ConstructionActivatorFactory constructor )
		{
			var parameter = new ConstructTypeRequest( typeof(ConstructedItem), 6776 );
			var first = constructor.Create( parameter );
			Assert.Same( first, constructor.Create( parameter ) );
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