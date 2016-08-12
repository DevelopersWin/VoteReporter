using DragonSpark.Activation;
using DragonSpark.Sources.Parameterized;
using Ploeh.AutoFixture.Xunit2;
using Xunit;

namespace DragonSpark.Testing.Activation
{
	public class ArgumentCacheTests
	{
		[Theory, AutoData]
		public void CreateDefault( Constructor sut )
		{
			var type = typeof(bool);
			var result = sut.Create<object>( type );
			Assert.Equal( false, result );
		}
	}
}