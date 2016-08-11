using DragonSpark.Configuration;
using DragonSpark.Testing.Framework;
using DragonSpark.Testing.Framework.Setup;
using DragonSpark.Testing.Objects.Properties;
using Ploeh.AutoFixture.Xunit2;
using Xunit;

namespace DragonSpark.Windows.Testing.Configuration
{
	[Trait( Traits.Category, Traits.Categories.ServiceLocation ), FrameworkTypes]
	public class ConfigurationTests
	{
		[Theory, DragonSpark.Testing.Framework.Setup.AutoData]
		public void FromConfiguration( DragonSpark.Testing.Objects.Configuration.Values sut )
		{
			var settings = Settings.Default;
			var primary = sut.Get( "PrimaryKey" );
			Assert.Equal( settings.HelloWorld, primary );

			var alias = sut.Get( "Some Key" );
			Assert.Equal( settings.HelloWorld, alias );
		}

		[Theory, DragonSpark.Testing.Framework.Setup.AutoData]
		[Map( typeof(IValueStore), typeof(DragonSpark.Testing.Objects.Configuration.Values) )]
		public void FromItem( [NoAutoProperties]DragonSpark.Testing.Objects.Configuration.Item sut )
		{
			Assert.Equal( "This is a value from a MemberInfoKey", sut.SomeTestingProperty );
		}
	}
}