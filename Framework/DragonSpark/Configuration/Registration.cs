using DragonSpark.Extensions;
using DragonSpark.Runtime;
using DragonSpark.TypeSystem;
using PostSharp.Patterns.Contracts;
using System;
using System.Linq;
using System.Windows.Markup;

namespace DragonSpark.Configuration
{
	[ContentProperty( nameof(Aliases) )]
	public class Registration : IEquatable<string>
	{
		public Registration() {}

		public Registration( [NotEmpty]string key, [Required]object value ) : this( key, value, Default<string>.Items ) {}

		public Registration( [NotEmpty]string key, [Required]object value, [Required]params string[] aliases )
		{
			Key = key;
			Aliases = new Collection<string>( aliases );
			Value = value;
		}

		public Collection<string> Aliases { get; } = new Collection<string>();

		[NotEmpty]
		public string Key { [return: NotEmpty]get; set; }

		[Required]
		public object Value { [return: Required]get; set; }

		public override bool Equals( object obj ) => obj.AsTo<string, bool>( Equals );

		public virtual bool Equals( string other ) => Key.Append( Aliases ).Contains( other );

		public override int GetHashCode() => 0;
	}
}
