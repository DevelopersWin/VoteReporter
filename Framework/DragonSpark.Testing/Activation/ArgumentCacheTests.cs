using DragonSpark.Activation;
using DragonSpark.Extensions;
using Ploeh.AutoFixture.Xunit2;
using Xunit;

namespace DragonSpark.Testing.Activation
{
	public class ArgumentCacheTests
	{
		[Theory, AutoData]
		public void CreateDefault()
		{
			var type = typeof(bool);
			var result = Constructor.Default.Get<object>( type );
			Assert.Equal( false, result );
		}
	}
}