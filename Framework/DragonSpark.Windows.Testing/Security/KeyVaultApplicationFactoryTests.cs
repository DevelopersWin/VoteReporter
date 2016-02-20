using System;
using DragonSpark.Windows.Security;
using Xunit;

namespace DragonSpark.Windows.Testing.Security
{
	public class KeyVaultApplicationFactoryTests
	{
		[Theory, DragonSpark.Testing.Framework.Setup.AutoData]
		public void Create( KeyVaultApplicationFactory sut, Uri location, Guid id )
		{
			var input = $"cn={location.DnsSafeHost},OU={id}";

			var created = sut.Create( input );
			Assert.Equal( id, created.Id );
			Assert.Equal( $"https://{location.Host}/", created.Location.ToString() );
		}
	}
}
