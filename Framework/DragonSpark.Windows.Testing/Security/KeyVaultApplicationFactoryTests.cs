using DragonSpark.Windows.Security;
using DragonSpark.Windows.Testing.Properties;
using Ploeh.AutoFixture.Xunit2;
using System;
using System.Security.Cryptography.X509Certificates;
using Xunit;

namespace DragonSpark.Windows.Testing.Security
{
	public class KeyVaultApplicationFactoryTests
	{
		[Theory, DragonSpark.Testing.Framework.Setup.AutoData]
		public void Create( [NoAutoProperties]X509Certificate2 certificate, KeyVaultApplicationFactory sut )
		{
			certificate.Import( Resources.Certificate );
		
			var created = sut.Create( certificate );
			Assert.Equal( new Guid("EB021F85-8102-410C-8925-FA886B4FF5B6"), created.Id );
			Assert.Equal( "https://security.testing.framework.dragonspark.us/", created.Location.ToString() );
			Assert.Equal( certificate, created.Certificate );
		}
	}
}
