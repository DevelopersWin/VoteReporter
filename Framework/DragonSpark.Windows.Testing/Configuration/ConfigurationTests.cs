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
		public void FromConfiguration( Configuration sut )
		{
			var temp = sut.Get( "PrimaryKey" );
			Assert.Equal( Settings.Default.HelloWorld, temp );
		}

		[Theory, DragonSpark.Testing.Framework.Setup.AutoData]
		[Map( typeof(IConfigurationRegistry), typeof(Configuration) )]
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