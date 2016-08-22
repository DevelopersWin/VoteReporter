using DragonSpark.Activation;
using Ploeh.AutoFixture.Xunit2;
using Xunit;

namespace DragonSpark.Testing.Activation
{
	public class LocatorTests
	{
		[Theory, AutoData]
		void CachesAsExpected( ConstructorStore constructorStore, int number )
		{
			var parameter = new ConstructTypeRequest( typeof(ConstructedItem), number );
			var first = constructorStore.Get( parameter );
			Assert.Same( first, constructorStore.Get( parameter ) );
			Assert.Equal( number, new ConstructedItem( number ).Number );
		}

		class ConstructedItem
		{
			public ConstructedItem( int number )
			{
				Number = number;
			}

			public int Number { get; set; }
		}
	}
}