using DragonSpark.Windows.Security;
using Ploeh.AutoFixture.Xunit2;
using Xunit;

namespace DragonSpark.Windows.Testing.Security
{
	public class SecretFactoryTests
	{
		[Theory, AutoData]
		public void Create( CertificateFactory certificateFactory, KeyVaultApplicationFactory applicationFactory )
		{
			var certificate = certificateFactory.Create( "" );
			var application = applicationFactory.Create( certificate );
			var sut = new SecretFactory( application );
			var secret = sut.Create( "HelloWorld" );
			Assert.NotEmpty( secret );
			Assert.Equal( "This is a message!", secret );
		}
	}
}