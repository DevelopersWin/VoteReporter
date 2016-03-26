using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using CPI.DirectoryServices;
using DragonSpark.Activation;
using DragonSpark.Extensions;
using PostSharp.Patterns.Contracts;

namespace DragonSpark.Windows.Security
{
	public class KeyVaultApplication
	{
		public KeyVaultApplication( [Required]X509Certificate2 certificate, [Required]Guid id, [Required]Uri location )
		{
			Certificate = certificate;
			Id = id;
			Location = location;
		}

		public X509Certificate2 Certificate { get; }
		public Guid Id { get; }
		public Uri Location { get; }
	}

	public class KeyVaultApplicationFactory : FactoryBase<X509Certificate2, KeyVaultApplication>
	{
		public static KeyVaultApplicationFactory Instance { get; } = new KeyVaultApplicationFactory();

		protected override KeyVaultApplication CreateItem( X509Certificate2 parameter )
		{
			var name = new DN( parameter.IssuerName.Name );
			var components = name.RDNs.SelectMany( rdn => rdn.Components ).ToArray();
			var id = Get( components, "OU" );
			var location = Get( components, "CN" );
			var result = new KeyVaultApplication( parameter, new Guid( id ), new Uri( $"{Uri.UriSchemeHttps}://{location}" ) );
			return result;
		}

		static string Get( IEnumerable<RDNComponent> components, string name ) => components.WithFirst( component => string.Equals( component.ComponentType, name, StringComparison.CurrentCultureIgnoreCase ), component => component.ComponentValue );
	}
}