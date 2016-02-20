using System;
using System.Collections.Generic;
using System.Linq;
using CPI.DirectoryServices;
using DragonSpark.Activation.FactoryModel;
using DragonSpark.Extensions;
using PostSharp.Patterns.Contracts;

namespace DragonSpark.Windows.Security
{
	public class KeyVaultApplication
	{
		public KeyVaultApplication( [Required]Guid id, [Required]Uri location )
		{
			Id = id;
			Location = location;
		}

		public Guid Id { get; set; }
		public Uri Location { get; set; }
	}

	public class KeyVaultApplicationFactory : FactoryBase<string, KeyVaultApplication>
	{
		public static KeyVaultApplicationFactory Instance { get; } = new KeyVaultApplicationFactory();

		protected override KeyVaultApplication CreateItem( string parameter )
		{
			var name = new DN( parameter );
			var components = name.RDNs.SelectMany( rdn => rdn.Components ).ToArray();
			var id = Get( components, "OU" );
			var location = Get( components, "CN" );
			var result = new KeyVaultApplication( new Guid( id ), new Uri( $"{Uri.UriSchemeHttps}://{location}" ) );
			return result;
		}

		static string Get( IEnumerable<RDNComponent> components, string name ) => components.WithFirst( component => string.Equals( component.ComponentType, name, StringComparison.CurrentCultureIgnoreCase ), component => component.ComponentValue );
	}
}