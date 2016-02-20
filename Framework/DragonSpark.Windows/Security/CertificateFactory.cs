using DragonSpark.Activation.FactoryModel;
using DragonSpark.Extensions;
using Microsoft.Azure.KeyVault;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using PostSharp.Patterns.Contracts;
using System;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace DragonSpark.Windows.Security
{
	public class KeyVaultClientFactory : FactoryBase<KeyVaultClient>
	{
		readonly ClientAssertionCertificate certificate;

		public KeyVaultClientFactory( [Required]KeyVaultApplication application ) : this( new ClientAssertionCertificate( application.Id.ToString(), application.Certificate ) ) {}

		public KeyVaultClientFactory( [Required]ClientAssertionCertificate certificate )
		{
			this.certificate = certificate;
		}

		protected override KeyVaultClient CreateItem() => new KeyVaultClient( GetToken );

		async Task<string> GetToken(string authority, string resource, string scope)
		{
			var authContext = new AuthenticationContext( authority );
			var result = await authContext.AcquireTokenAsync( resource, certificate );

			if ( result == null )
			{
				throw new InvalidOperationException( "Failed to obtain the JWT token" );
			}

			return result.AccessToken;
		}
	}

	public class SecretFactory : FactoryBase<string, string>
	{
		readonly KeyVaultClient client;
		readonly Uri location;

		public SecretFactory( [Required]KeyVaultApplication application ) : this( new KeyVaultClientFactory( application ).Create(), application.Location ) {}

		public SecretFactory( [Required]KeyVaultClient client, [Required]Uri location )
		{
			this.client = client;
			this.location = location;
		}

		protected override string CreateItem( string parameter ) => client.GetSecretAsync( location.ToString(), parameter ).Result.Value;
	}

	public class CertificateFactory : FactoryBase<string, X509Certificate2>
	{
		public static CertificateFactory Instance { get; } = new CertificateFactory();

		readonly X509Store store;

		public CertificateFactory() : this( new X509Store( StoreName.My, StoreLocation.LocalMachine ) ) {}

		public CertificateFactory( [Required]X509Store store )
		{
			this.store = store;
		}

		protected override X509Certificate2 CreateItem( string parameter )
		{
			try
			{
				store.Open(OpenFlags.ReadOnly);
				var result = store.Certificates.Find( X509FindType.FindByThumbprint, parameter, false ) // Don't validate certs, since the test root isn't installed.
					.With( collection => collection.Cast<X509Certificate2>().FirstOrDefault() );
				return result;
			}
			finally
			{
				store.Close();
			}
		}
	}
}