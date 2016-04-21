using DragonSpark.Configuration;
using DragonSpark.Testing.Framework;
using DragonSpark.Windows.Testing.Properties;
using Ploeh.AutoFixture.Xunit2;
using Xunit;

namespace DragonSpark.Windows.Testing.Configuration
{
	public class ConfigurationTests
	{
		[Theory, DragonSpark.Testing.Framework.Setup.AutoData]
		public void FromConfiguration( Values sut )
		{
			var primary = sut.Get( "PrimaryKey" );
			Assert.Equal( Settings.Default.HelloWorld, primary );

			var alias = sut.Get( "Some Key" );
			Assert.Equal( Settings.Default.HelloWorld, alias );
		}

		[Theory, DragonSpark.Testing.Framework.Setup.AutoData]
		[Map( typeof(IValueStore), typeof(Values) )]
		public void FromItem( [NoAutoProperties]Item sut )
		{
			Assert.Equal( "This is a value from a MemberInfoKey", sut.SomeTestingProperty );
		}
	}

	public class TestItem
	{
		public string SomeTestingProperty { get; set; }
	}
}