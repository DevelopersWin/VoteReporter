using DragonSpark.Windows.Security;
using Ploeh.AutoFixture.Xunit2;
using Xunit;

namespace DragonSpark.Windows.Testing.Security
{
	public class SecretFactoryTests
	{
		[Theory, AutoData]
		public void Create( CertificateFactory certificateFactory, KeyVaultApplicationFactory applicationFactory )
		{}
	}
}