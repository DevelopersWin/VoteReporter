using System;
using System.Collections.Generic;
using PostSharp.Patterns.Contracts;
using System.Collections.ObjectModel;
using System.Composition;
using System.Linq;
using DragonSpark.Extensions;

namespace DragonSpark.Configuration
{
	public interface IConfigurationRegistry
	{
		object Get( string key );
	}

	[Export( typeof(IConfigurationRegistry) )]
	public class ConfigurationRegistry : KeyedCollection<string, Registration>, IConfigurationRegistry
	{
		public object Get( [Required]string key ) => this.WithFirst( registration => registration.Equals( key ), registration => registration.Value );

		protected override string GetKeyForItem( Registration item ) => item.Key;
	}

	/*public class RegistrationKey : IEquatable<string>
	{
		readonly IEnumerable<string> items;

		public RegistrationKey( [Required]Registration registration ) : this( registration.Key.Append( registration.Aliases ) ) {}

		public RegistrationKey( [Required]IEnumerable<string> items )
		{
			this.items = items.Fixed();
		}

		public override bool Equals( object obj ) => obj.AsTo<string, bool>( Equals );

		public virtual bool Equals( string other ) => items.Contains( other );

		public override int GetHashCode() => 0;
	}*/
}