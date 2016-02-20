using DragonSpark.Testing.Framework.Setup;
using DragonSpark.Windows.Security;
using Xunit;

namespace DragonSpark.Windows.Testing.Security
{
	public class CertificateFactoryTests
	{
		[Theory, AutoData]
		public void Create( CertificateFactory sut )
		{
			var result = sut.Create( "68OxxjVSjO9T44v/1GXTbAYINZ8=" );
			Assert.NotNull( result );
		}
	}
}
