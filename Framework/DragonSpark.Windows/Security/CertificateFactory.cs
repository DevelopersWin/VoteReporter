using DragonSpark.Activation;
using DragonSpark.Extensions;
using PostSharp.Patterns.Contracts;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using DragonSpark.Sources.Parameterized;

namespace DragonSpark.Windows.Security
{
	public class CertificateFactory : FactoryBase<string, X509Certificate2>
	{
		public static CertificateFactory Instance { get; } = new CertificateFactory();

		readonly X509Store store;

		public CertificateFactory() : this( new X509Store( StoreName.My, StoreLocation.LocalMachine ) ) {}

		public CertificateFactory( [Required]X509Store store )
		{
			this.store = store;
		}

		public override X509Certificate2 Create( string parameter )
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
}