using System;
using System.Linq;
using System.Runtime.InteropServices;

namespace DragonSpark.Extensions
{
	public static class StringExtensions
	{
		public static string Capitalized( this string target ) => string.IsNullOrEmpty( target ) ? string.Empty : $"{char.ToUpper( target[0] )}{target.Substring( 1 )}";

		public static string NullIfEmpty( [Optional]this string target ) => string.IsNullOrEmpty( target ) ? null : target;

		public static string[] ToStringArray( this string target ) => ToStringArray( target, ',', ';' );

		public static string[] ToStringArray( this string target, params char[] delimiters )
		{
			var items =
				from item in ( target ?? string.Empty ).Split( delimiters, StringSplitOptions.RemoveEmptyEntries )
				select item.Trim();
			var result = items.ToArray();
			return result;
		}

		public static string TrimStartOf( this string @this, params char[] chars ) => chars.Select( c => c.ToString() ).Any( @this.StartsWith ) ? @this.Substring( 1 ) : @this;
	}

	// ATTRIBUTION: http://stackoverflow.com/questions/773303/splitting-camelcase
}