using DragonSpark.Extensions;
using DragonSpark.Sources.Parameterized;
using System.Linq;
using System.Security.Cryptography.X509Certificates;

namespace DragonSpark.Windows.Security
{
	public class CertificateFactory : ParameterizedSourceBase<string, X509Certificate2>
	{
		public static CertificateFactory Default { get; } = new CertificateFactory();

		readonly X509Store store;

		public CertificateFactory() : this( new X509Store( StoreName.My, StoreLocation.LocalMachine ) ) {}

		public CertificateFactory( X509Store store )
		{
			this.store = store;
		}

		public override X509Certificate2 Get( string parameter )
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