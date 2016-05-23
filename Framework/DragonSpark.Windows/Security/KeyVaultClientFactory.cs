namespace DragonSpark.Windows.Security
{
	/*public class KeyVaultClientFactory : FactoryBase<KeyVaultClient>
	{
		readonly ClientAssertionCertificate certificate;

		public KeyVaultClientFactory( [Required]KeyVaultApplication application ) : this( new ClientAssertionCertificate( application.Id.ToString(), application.Certificate ) ) {}

		public KeyVaultClientFactory( [Required]ClientAssertionCertificate certificate )
		{
			this.certificate = certificate;
		}

		public override KeyVaultClient Create() => new KeyVaultClient( GetToken );

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

		public override string Create( string parameter ) => client.GetSecretAsync( location.ToString(), parameter ).Result.Value;
	}

	public class SaveCertificateCommand : CommandBase<SaveCertificateCommand.Parameter>
	{
		public class Parameter
		{
			public Parameter( Org.BouncyCastle.X509.X509Certificate certificate, AsymmetricCipherKeyPair keyPair, string filePath, string certAlias, string password )
			{
				Certificate = certificate;
				KeyPair = keyPair;
				FilePath = filePath;
				CertAlias = certAlias;
				Password = password;
			}

			public Org.BouncyCastle.X509.X509Certificate Certificate { get; }
			public AsymmetricCipherKeyPair KeyPair { get; }
			public string FilePath { get; }
			public string CertAlias { get; }
			public string Password { get; }
		}

		public override void Execute( Parameter parameter )
		{
			var entry = new X509CertificateEntry( parameter.Certificate );

			
 
			var store = new Pkcs12Store();
			// store.Load();
			store.SetCertificateEntry( parameter.CertAlias, entry );
			store.SetKeyEntry( parameter.CertAlias, new AsymmetricKeyEntry( parameter.KeyPair.Private ), new[] { entry } );
 
			/*using ( var certFile = File.Create( parameter.FilePath ) )
			{
				store.Save( certFile, parameter.Password.ToCharArray(), new SecureRandom( new CryptoApiRandomGenerator() ) );
			}#1#

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

		public override AsymmetricCipherKeyPair Create( int parameter )
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

		public override Org.BouncyCastle.X509.X509Certificate Create( Parameter parameter )
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
			 #1#
			generator.AddExtension( X509Extensions.ExtendedKeyUsage.Id, false, new ExtendedKeyUsage( KeyPurposeID.IdKPCodeSigning ) );
 
			var result = generator.Generate( parameter.KeyPair.Private );
			return result;
		}
	}*/
}