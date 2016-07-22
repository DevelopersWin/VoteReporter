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

		public Registration( [NotEmpty]string key, [Required]object value ) : this( key, value, Items<string>.Default ) {}

		public Registration( [NotEmpty]string key, [Required]object value, [Required]params string[] aliases )
		{
			Key = key;
			Aliases = new DeclarativeCollection<string>( aliases );
			Value = value;
		}

		public DeclarativeCollection<string> Aliases { get; } = new DeclarativeCollection<string>();

		[NotEmpty]
		public string Key { [return: NotEmpty]get; set; }

		[Required]
		public object Value { [return: Required]get; set; }

		public override bool Equals( object obj ) => obj.AsTo<string, bool>( Equals );

		public virtual bool Equals( string other ) => Key.Append( Aliases ).Contains( other );

		public override int GetHashCode() => 0;
	}
}
