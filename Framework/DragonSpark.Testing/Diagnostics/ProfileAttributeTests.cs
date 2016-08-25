using DragonSpark.Aspects;
using DragonSpark.Testing.Framework;
using Xunit.Abstractions;

namespace DragonSpark.Testing.Diagnostics
{
	public class ProfileAttributeTests : TestCollectionBase
	{
		const string OverridingMethodTemplate = "Overriding Method Template";
		public ProfileAttributeTests( ITestOutputHelper output ) : base( output ) {}

		/*[Fact]
		public void Verify()
		{
			var history = LoggingHistory.Default.Get();
			Assert.Empty( history.Events );
			HelloWorld();
			var item = Assert.Single( history.Events );
			var text = item.MessageTemplate.Text;
			Assert.Contains( OverridingMethodTemplate, text );
		}*/

		[Timed( OverridingMethodTemplate )]
		static void HelloWorld() {}
	}
}
