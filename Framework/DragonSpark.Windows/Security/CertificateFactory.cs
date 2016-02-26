using DragonSpark.Activation.FactoryModel;
using DragonSpark.Extensions;
using DragonSpark.Runtime;
using Microsoft.Azure.KeyVault;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Prng;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.X509;
using PostSharp.Patterns.Contracts;
using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using X509Certificate = Org.BouncyCastle.X509.X509Certificate;

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
				store.Open(OpenFlags.ReadWrite);
				
				var result = store.Certificates.Find( X509FindType.FindByThumbprint, parameter, false )
					.With( collection => collection.Cast<X509Certificate2>().FirstOrDefault() );
				return result;
			}
			finally
			{
				store.Close();
			}
		}
	}
	
	public class SaveCertificateCommand : Command<SaveCertificateCommand.Parameter>
	{
		public class Parameter
		{
			public Parameter( X509Certificate certificate, AsymmetricCipherKeyPair keyPair, string filePath, string certAlias, string password )
			{
				Certificate = certificate;
				KeyPair = keyPair;
				FilePath = filePath;
				CertAlias = certAlias;
				Password = password;
			}

			public X509Certificate Certificate { get; }
			public AsymmetricCipherKeyPair KeyPair { get; }
			public string FilePath { get; }
			public string CertAlias { get; }
			public string Password { get; }
		}

		protected override void OnExecute( Parameter parameter )
		{
			var entry = new X509CertificateEntry( parameter.Certificate );

			
 
			var store = new Pkcs12Store();
			// store.Load();
			store.SetCertificateEntry( parameter.CertAlias, entry );
			store.SetKeyEntry( parameter.CertAlias, new AsymmetricKeyEntry( parameter.KeyPair.Private ), new[] { entry } );
 
			/*using ( var certFile = File.Create( parameter.FilePath ) )
			{
				store.Save( certFile, parameter.Password.ToCharArray(), new SecureRandom( new CryptoApiRandomGenerator() ) );
			}*/

			var stream = new MemoryStream();
			store.Save( stream, parameter.Password.ToCharArray(), new SecureRandom( new CryptoApiRandomGenerator() ) );

			var certificate = new X509Certificate2( stream.ToArray(), parameter.Password, X509KeyStorageFlags.PersistKeySet | X509KeyStorageFlags.Exportable );
			
			// var temp = new Guid( new BigInteger( certificate.SerialNumber, 16 ).ToByteArrayUnsigned() );

			File.WriteAllBytes( parameter.FilePath, certificate.Export( X509ContentType.Cert, parameter.Password ) );
		}
	}

	class RsaKeyPairFactory : FactoryBase<int, AsymmetricCipherKeyPair>
	{
		public static RsaKeyPairFactory Instance { get; } = new RsaKeyPairFactory();

		protected override AsymmetricCipherKeyPair CreateItem( int parameter )
		{
			var generator = new RsaKeyPairGenerator();
			var parameters = new KeyGenerationParameters( new SecureRandom( new CryptoApiRandomGenerator() ), parameter );
			generator.Init( parameters );
			var result = generator.GenerateKeyPair();
			return result;
		}
	}

	public class CertificateContextFactory : FactoryBase<CertificateContextFactory.Parameter, Org.BouncyCastle.X509.X509Certificate>
	{
		public class Parameter
		{
			public Parameter( string commonName ) : this( 1024, commonName ) {}

			public Parameter( int strength, string commonName ) : this( RsaKeyPairFactory.Instance.Create( strength ), commonName ) {}
			
			public Parameter( [Required]AsymmetricCipherKeyPair keyPair, [NotEmpty]string commonName )
			{
				KeyPair = keyPair;
				CommonName = commonName;
			}

			public AsymmetricCipherKeyPair KeyPair { get; }
			public string CommonName { get; }
		}

		public class Result
		{
			public Result( [Required]X509Certificate certificate, [Required]AsymmetricCipherKeyPair keyPair )
			{
				Certificate = certificate;
				KeyPair = keyPair;
			}

			public X509Certificate Certificate { get; }
			public AsymmetricCipherKeyPair KeyPair { get; }
		}

		protected override X509Certificate CreateItem( Parameter parameter )
		{
			var generator = new X509V3CertificateGenerator();
 
			var name = new X509Name( $"CN={parameter.CommonName}" );
			// var number = BigInteger.ProbablePrime( 120, new Random() );

			var array = new Guid( "7A842B3A-0F96-4A0E-8D49-BD62DDB6EB38" ).ToByteArray();
			var number = new BigInteger( 1, array );

			generator.SetSerialNumber( number );
			generator.SetSubjectDN( name );
			generator.SetIssuerDN( name );
			generator.SetNotAfter( DateTime.Now.AddYears( 100 ) );
			generator.SetNotBefore( DateTime.Now.Subtract( new TimeSpan( 7, 0, 0, 0 ) ) );
			generator.SetSignatureAlgorithm( "SHA512withRSA" );
			generator.SetPublicKey( parameter.KeyPair.Public );

			var keyInfo = SubjectPublicKeyInfoFactory.CreateSubjectPublicKeyInfo( parameter.KeyPair.Public );
			var identifier = new AuthorityKeyIdentifier( keyInfo, new GeneralNames( new GeneralName( name ) ), number );
			generator.AddExtension( X509Extensions.AuthorityKeyIdentifier.Id, false, identifier );
 
			generator.AddExtension( new DerObjectIdentifier( "0.6.7.7.6.0" ), true, Encoding.Default.GetBytes( "http://yahoo.com" ) );
			

			/* 
			 1.3.6.1.5.5.7.3.1 - id_kp_serverAuth 
			 1.3.6.1.5.5.7.3.2 - id_kp_clientAuth 
			 1.3.6.1.5.5.7.3.3 - id_kp_codeSigning 
			 1.3.6.1.5.5.7.3.4 - id_kp_emailProtection 
			 1.3.6.1.5.5.7.3.5 - id-kp-ipsecEndSystem 
			 1.3.6.1.5.5.7.3.6 - id-kp-ipsecTunnel 
			 1.3.6.1.5.5.7.3.7 - id-kp-ipsecUser 
			 1.3.6.1.5.5.7.3.8 - id_kp_timeStamping 
			 1.3.6.1.5.5.7.3.9 - OCSPSigning
			 */
			generator.AddExtension( X509Extensions.ExtendedKeyUsage.Id, false, new ExtendedKeyUsage( KeyPurposeID.IdKPCodeSigning ) );
 
			var result = generator.Generate( parameter.KeyPair.Private );
			return result;
		}
	}
}