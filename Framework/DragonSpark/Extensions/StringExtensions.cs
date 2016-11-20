using PostSharp.Patterns.Contracts;
using System;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.InteropServices;

namespace DragonSpark.Extensions
{
	public static class StringExtensions
	{
		public static string Capitalized( this string target ) => string.IsNullOrEmpty( target ) ? string.Empty : $"{char.ToUpper( target[0] ).ToString()}{target.Substring( 1 )}";

		public static string NullIfEmpty( [Optional]this string target ) => string.IsNullOrEmpty( target ) ? null : target;

		public static ImmutableArray<string> ToStringArray( [NotNull]this string target ) => ToStringArray( target, ',', ';' );

		public static ImmutableArray<string> ToStringArray( [NotNull]this string target, params char[] delimiters ) => 
			target.Split( delimiters, StringSplitOptions.RemoveEmptyEntries ).Select( s => s.Trim() ).ToImmutableArray();

		public static string TrimStartOf( this string @this, params char[] characters )
		{
			foreach ( var c in characters )
			{
				if ( @this.StartsWith( c.ToString() ) )
				{
					return @this.Substring( 1 );
				}
			}

			return @this;
		}
	}
}