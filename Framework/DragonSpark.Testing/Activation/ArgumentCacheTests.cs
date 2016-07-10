using DragonSpark.Activation;
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
			var result = sut.CreateUsing<object>( type );
			Assert.Equal( false, result );
		}
	}
}