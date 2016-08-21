using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace DragonSpark.Aspects
{
	// [AutoValidation( false )]
	/*public class MemberInfoTransformer : Factory<MemberInfo, int>
	{
		public static MemberInfoTransformer Default { get; } = new MemberInfoTransformer();

		public MemberInfoTransformer() : base( IsTypeSpecification<MemberInfo>.Default ) {}

		protected override int CreateItem( MemberInfo parameter ) => parameter is TypeInfo
			? 
			parameter.GetHashCode() : 
			parameter.DeclaringType.GetTypeInfo().GUID.GetHashCode() * 6776 + parameter.ToString().GetHashCode();
	}

	public class ParameterInfoTransformer : Factory<ParameterInfo, int>
	{
		public static ParameterInfoTransformer Default { get; } = new ParameterInfoTransformer();

		public ParameterInfoTransformer() : base( IsTypeSpecification<ParameterInfo>.Default ) {}

		protected override int CreateItem( ParameterInfo parameter ) => 
			parameter.Member.DeclaringType.GetTypeInfo().GUID.GetHashCode() * 6776 + parameter.ToString().GetHashCode();
	}*/

	/*class AssociatedHash : AttachedPropertyBase<object, Tuple<int>>
	{
		public static AssociatedHash Default { get; } = new AssociatedHash();

		AssociatedHash() : base( key => new Tuple<int>( key.GetHashCode() ) ) {}
	}*/

	// ATTRIBUTION: https://github.com/dotnet/roslyn/blob/master/src/Compilers/Core/Portable/InternalUtilities/ValueTuple.cs
	public static class ValueTuple
	{
		public static ValueTuple<T1, T2> Create<T1, T2>( T1 item1, T2 item2 ) => new ValueTuple<T1, T2>( item1, item2 );

		public static ValueTuple<T1, T2, T3> Create<T1, T2, T3>( T1 item1, T2 item2, T3 item3 ) => new ValueTuple<T1, T2, T3>( item1, item2, item3 );
	}

	// ATTRIBUTION: https://github.com/dotnet/roslyn/blob/master/src/Compilers/Core/Portable/InternalUtilities/ValueTuple%603.cs
	public struct ValueTuple<T1, T2, T3> : IEquatable<ValueTuple<T1, T2, T3>>
	{
		readonly private static EqualityComparer<T1> Comparer1 = EqualityComparer<T1>.Default;
		readonly private static EqualityComparer<T2> Comparer2 = EqualityComparer<T2>.Default;
		readonly private static EqualityComparer<T3> Comparer3 = EqualityComparer<T3>.Default;

		public ValueTuple( T1 item1, T2 item2, T3 item3 )
		{
			Item1 = item1;
			Item2 = item2;
			Item3 = item3;
		}

		public T1 Item1 { get; }
		public T2 Item2 { get; }
		public T3 Item3 { get; }

		public bool Equals( ValueTuple<T1, T2, T3> other )
		{
			return Comparer1.Equals( Item1, other.Item1 )
				   && Comparer2.Equals( Item2, other.Item2 )
				   && Comparer3.Equals( Item3, other.Item3 );
		}

		public override bool Equals( object obj )
		{
			if ( obj is ValueTuple<T1, T2, T3> )
			{
				var other = (ValueTuple<T1, T2, T3>)obj;
				return Equals( other );
			}

			return false;
		}

		public override int GetHashCode()
		{
			return Hash.Combine(
				Hash.Combine(
					Comparer1.GetHashCode( Item1 ),
					Comparer2.GetHashCode( Item2 ) ),
				Comparer3.GetHashCode( Item3 ) );
		}

		public static bool operator ==( ValueTuple<T1, T2, T3> left, ValueTuple<T1, T2, T3> right )
		{
			return left.Equals( right );
		}

		public static bool operator !=( ValueTuple<T1, T2, T3> left, ValueTuple<T1, T2, T3> right )
		{
			return !left.Equals( right );
		}
	}

	// ATTRIBUTION: https://github.com/dotnet/roslyn/blob/master/src/Compilers/Core/Portable/InternalUtilities/ValueTuple%602.cs
	public struct ValueTuple<T1, T2> : IEquatable<ValueTuple<T1, T2>>
	{
		readonly static EqualityComparer<T1> Comparer1 = EqualityComparer<T1>.Default;
		readonly static EqualityComparer<T2> Comparer2 = EqualityComparer<T2>.Default;

		public ValueTuple( T1 item1, T2 item2 )
		{
			Item1 = item1;
			Item2 = item2;
		}

		public T1 Item1 { get; }
		public T2 Item2 { get; }

		public bool Equals( ValueTuple<T1, T2> other ) => Comparer1.Equals( Item1, other.Item1 ) && Comparer2.Equals( Item2, other.Item2 );

		public override bool Equals( object obj )
		{
			if ( obj is ValueTuple<T1, T2> )
			{
				var other = (ValueTuple<T1, T2>)obj;
				return this.Equals( other );
			}

			return false;
		}

		public override int GetHashCode() => Hash.Combine( Comparer1.GetHashCode( Item1 ), Comparer2.GetHashCode( Item2 ) );

		public static bool operator ==( ValueTuple<T1, T2> left, ValueTuple<T1, T2> right ) => left.Equals( right );

		public static bool operator !=( ValueTuple<T1, T2> left, ValueTuple<T1, T2> right ) => !left.Equals( right );
	}

	// ATTRIBUTION: https://github.com/dotnet/roslyn/blob/master/src/Compilers/Core/Portable/InternalUtilities/Hash.cs
	internal static class Hash
	{
		/// <summary>
		/// This is how VB Anonymous Types combine hash values for fields.
		/// </summary>
		internal static int Combine( int newKey, int currentKey ) => unchecked( ( currentKey * (int)0xA5555529 ) + newKey );

		internal static int Combine( bool newKeyPart, int currentKey ) => Combine( currentKey, newKeyPart ? 1 : 0 );

		/// <summary>
		/// This is how VB Anonymous Types combine hash values for fields.
		/// PERF: Do not use with enum types because that involves multiple
		/// unnecessary boxing operations.  Unfortunately, we can't constrain
		/// T to "non-enum", so we'll use a more restrictive constraint.
		/// </summary>
		internal static int Combine<T>( T newKeyPart, int currentKey ) where T : class
		{
			int hash = unchecked( currentKey * (int)0xA5555529 );

			if ( newKeyPart != null )
			{
				return unchecked( hash + newKeyPart.GetHashCode() );
			}

			return hash;
		}

		internal static int CombineValues<T>( IEnumerable<T> values, int maxItemsToHash = int.MaxValue )
		{
			if ( values == null )
			{
				return 0;
			}

			var hashCode = 0;
			var count = 0;
			foreach ( var value in values )
			{
				if ( count++ >= maxItemsToHash )
				{
					break;
				}

				// Should end up with a constrained virtual call to object.GetHashCode (i.e. avoid boxing where possible).
				if ( value != null )
				{
					hashCode = Combine( value.GetHashCode(), hashCode );
				}
			}

			return hashCode;
		}

		internal static int CombineValues<T>( T[] values, int maxItemsToHash = int.MaxValue )
		{
			if ( values == null )
			{
				return 0;
			}

			var maxSize = Math.Min( maxItemsToHash, values.Length );
			var hashCode = 0;

			for ( int i = 0; i < maxSize; i++ )
			{
				T value = values[i];

				// Should end up with a constrained virtual call to object.GetHashCode (i.e. avoid boxing where possible).
				if ( value != null )
				{
					hashCode = Hash.Combine( value.GetHashCode(), hashCode );
				}
			}

			return hashCode;
		}

		internal static int CombineValues<T>( ImmutableArray<T> values, int maxItemsToHash = int.MaxValue )
		{
			if ( values.IsDefaultOrEmpty )
			{
				return 0;
			}

			var hashCode = 0;
			var count = 0;
			foreach ( var value in values )
			{
				if ( count++ >= maxItemsToHash )
				{
					break;
				}

				// Should end up with a constrained virtual call to object.GetHashCode (i.e. avoid boxing where possible).
				if ( value != null )
				{
					hashCode = Combine( value.GetHashCode(), hashCode );
				}
			}

			return hashCode;
		}

		internal static int CombineValues( IEnumerable<string> values, StringComparer stringComparer, int maxItemsToHash = int.MaxValue )
		{
			if ( values == null )
			{
				return 0;
			}

			var hashCode = 0;
			var count = 0;
			foreach ( var value in values )
			{
				if ( count++ >= maxItemsToHash )
				{
					break;
				}

				if ( value != null )
				{
					hashCode = Hash.Combine( stringComparer.GetHashCode( value ), hashCode );
				}
			}

			return hashCode;
		}

		/// <summary>
		/// The offset bias value used in the FNV-1a algorithm
		/// See http://en.wikipedia.org/wiki/Fowler%E2%80%93Noll%E2%80%93Vo_hash_function
		/// </summary>
		const int FnvOffsetBias = unchecked( (int)2166136261 );

		/// <summary>
		/// The generative factor used in the FNV-1a algorithm
		/// See http://en.wikipedia.org/wiki/Fowler%E2%80%93Noll%E2%80%93Vo_hash_function
		/// </summary>
		const int FnvPrime = 16777619;

		/// <summary>
		/// Compute the FNV-1a hash of a sequence of bytes
		/// See http://en.wikipedia.org/wiki/Fowler%E2%80%93Noll%E2%80%93Vo_hash_function
		/// </summary>
		/// <param name="data">The sequence of bytes</param>
		/// <returns>The FNV-1a hash of <paramref name="data"/></returns>
		internal static int GetFNVHashCode( byte[] data )
		{
			int hashCode = Hash.FnvOffsetBias;

			for ( int i = 0; i < data.Length; i++ )
			{
				hashCode = unchecked( ( hashCode ^ data[i] ) * Hash.FnvPrime );
			}

			return hashCode;
		}

		/*/// <summary>
			/// Compute the FNV-1a hash of a sequence of bytes and determines if the byte
			/// sequence is valid ASCII and hence the hash code matches a char sequence
			/// encoding the same text.
			/// See http://en.wikipedia.org/wiki/Fowler%E2%80%93Noll%E2%80%93Vo_hash_function
			/// </summary>
			/// <param name="data">The sequence of bytes that are likely to be ASCII text.</param>
			/// <param name="length">The length of the sequence.</param>
			/// <param name="isAscii">True if the sequence contains only characters in the ASCII range.</param>
			/// <returns>The FNV-1a hash of <paramref name="data"/></returns>
			internal static unsafe int GetFNVHashCode( byte* data, int length, out bool isAscii )
			{
				int hashCode = Hash.FnvOffsetBias;

				byte asciiMask = 0;

				for ( int i = 0; i < length; i++ )
				{
					byte b = data[i];
					asciiMask |= b;
					hashCode = unchecked( ( hashCode ^ b ) * Hash.FnvPrime );
				}

				isAscii = ( asciiMask & 0x80 ) == 0;
				return hashCode;
			}*/

		/// <summary>
		/// Compute the FNV-1a hash of a sequence of bytes
		/// See http://en.wikipedia.org/wiki/Fowler%E2%80%93Noll%E2%80%93Vo_hash_function
		/// </summary>
		/// <param name="data">The sequence of bytes</param>
		/// <returns>The FNV-1a hash of <paramref name="data"/></returns>
		internal static int GetFNVHashCode( ImmutableArray<byte> data )
		{
			int hashCode = Hash.FnvOffsetBias;

			for ( int i = 0; i < data.Length; i++ )
			{
				hashCode = unchecked( ( hashCode ^ data[i] ) * Hash.FnvPrime );
			}

			return hashCode;
		}

		/// <summary>
		/// Compute the hashcode of a sub-string using FNV-1a
		/// See http://en.wikipedia.org/wiki/Fowler%E2%80%93Noll%E2%80%93Vo_hash_function
		/// Note: FNV-1a was developed and tuned for 8-bit sequences. We're using it here
		/// for 16-bit Unicode chars on the understanding that the majority of chars will
		/// fit into 8-bits and, therefore, the algorithm will retain its desirable traits
		/// for generating hash codes.
		/// </summary>
		/// <param name="text">The input string</param>
		/// <param name="start">The start index of the first character to hash</param>
		/// <param name="length">The number of characters, beginning with <paramref name="start"/> to hash</param>
		/// <returns>The FNV-1a hash code of the substring beginning at <paramref name="start"/> and ending after <paramref name="length"/> characters.</returns>
		internal static int GetFNVHashCode( string text, int start, int length )
		{
			int hashCode = Hash.FnvOffsetBias;
			int end = start + length;

			for ( int i = start; i < end; i++ )
			{
				hashCode = unchecked( ( hashCode ^ text[i] ) * Hash.FnvPrime );
			}

			return hashCode;
		}

		/// <summary>
		/// Compute the hashcode of a sub-string using FNV-1a
		/// See http://en.wikipedia.org/wiki/Fowler%E2%80%93Noll%E2%80%93Vo_hash_function
		/// </summary>
		/// <param name="text">The input string</param>
		/// <param name="start">The start index of the first character to hash</param>
		/// <returns>The FNV-1a hash code of the substring beginning at <paramref name="start"/> and ending at the end of the string.</returns>
		internal static int GetFNVHashCode( string text, int start ) => GetFNVHashCode( text, start, length: text.Length - start );

		/// <summary>
		/// Compute the hashcode of a string using FNV-1a
		/// See http://en.wikipedia.org/wiki/Fowler%E2%80%93Noll%E2%80%93Vo_hash_function
		/// </summary>
		/// <param name="text">The input string</param>
		/// <returns>The FNV-1a hash code of <paramref name="text"/></returns>
		internal static int GetFNVHashCode( string text ) => CombineFNVHash( Hash.FnvOffsetBias, text );

		/// <summary>
		/// Compute the hashcode of a string using FNV-1a
		/// See http://en.wikipedia.org/wiki/Fowler%E2%80%93Noll%E2%80%93Vo_hash_function
		/// </summary>
		/// <param name="text">The input string</param>
		/// <returns>The FNV-1a hash code of <paramref name="text"/></returns>
		internal static int GetFNVHashCode( System.Text.StringBuilder text )
		{
			int hashCode = Hash.FnvOffsetBias;
			int end = text.Length;

			for ( int i = 0; i < end; i++ )
			{
				hashCode = unchecked( ( hashCode ^ text[i] ) * Hash.FnvPrime );
			}

			return hashCode;
		}

		/// <summary>
		/// Compute the hashcode of a sub string using FNV-1a
		/// See http://en.wikipedia.org/wiki/Fowler%E2%80%93Noll%E2%80%93Vo_hash_function
		/// </summary>
		/// <param name="text">The input string as a char array</param>
		/// <param name="start">The start index of the first character to hash</param>
		/// <param name="length">The number of characters, beginning with <paramref name="start"/> to hash</param>
		/// <returns>The FNV-1a hash code of the substring beginning at <paramref name="start"/> and ending after <paramref name="length"/> characters.</returns>
		internal static int GetFNVHashCode( char[] text, int start, int length )
		{
			int hashCode = Hash.FnvOffsetBias;
			int end = start + length;

			for ( int i = start; i < end; i++ )
			{
				hashCode = unchecked( ( hashCode ^ text[i] ) * Hash.FnvPrime );
			}

			return hashCode;
		}

		/// <summary>
		/// Compute the hashcode of a single character using the FNV-1a algorithm
		/// See http://en.wikipedia.org/wiki/Fowler%E2%80%93Noll%E2%80%93Vo_hash_function
		/// Note: In general, this isn't any more useful than "char.GetHashCode". However,
		/// it may be needed if you need to generate the same hash code as a string or
		/// substring with just a single character.
		/// </summary>
		/// <param name="ch">The character to hash</param>
		/// <returns>The FNV-1a hash code of the character.</returns>
		internal static int GetFNVHashCode( char ch ) => CombineFNVHash( FnvOffsetBias, ch );

		/// <summary>
		/// Combine a string with an existing FNV-1a hash code
		/// See http://en.wikipedia.org/wiki/Fowler%E2%80%93Noll%E2%80%93Vo_hash_function
		/// </summary>
		/// <param name="hashCode">The accumulated hash code</param>
		/// <param name="text">The string to combine</param>
		/// <returns>The result of combining <paramref name="hashCode"/> with <paramref name="text"/> using the FNV-1a algorithm</returns>
		internal static int CombineFNVHash( int hashCode, string text )
		{
			foreach ( char ch in text )
			{
				hashCode = unchecked( ( hashCode ^ ch ) * Hash.FnvPrime );
			}

			return hashCode;
		}

		/// <summary>
		/// Combine a char with an existing FNV-1a hash code
		/// See http://en.wikipedia.org/wiki/Fowler%E2%80%93Noll%E2%80%93Vo_hash_function
		/// </summary>
		/// <param name="hashCode">The accumulated hash code</param>
		/// <param name="ch">The new character to combine</param>
		/// <returns>The result of combining <paramref name="hashCode"/> with <paramref name="ch"/> using the FNV-1a algorithm</returns>
		internal static int CombineFNVHash( int hashCode, char ch ) => unchecked( ( hashCode ^ ch ) * FnvPrime );
	}

	public sealed class KeyFactory //  : KeyFactory<int>
	{
		// public static KeyFactory Default { get; } = new KeyFactory();

		// public string ToString( params object[] items ) => Create( items ).ToString();

		public static int CreateUsing( params object[] parameter ) => Create( ImmutableArray.Create( parameter ) );

		public static int Create( ImmutableArray<object> parameter )
		{
			var result = Hash.CombineValues( Expand( parameter ) );
			return result;

			/*var result = 0x2D2816FE;
			for ( var i = 0; i < parameter.Length; i++ )
			{
				var next = result * 31;
				var item = parameter[i];
				var increment = item != null ? GetCode( item ) : 0;
				result += next + increment;
			}
			return result;*/
		}

		static ImmutableArray<object> Expand( ImmutableArray<object> current )
		{
			var builder = current.ToBuilder();
			foreach ( var o in current )
			{
				var list = o as IList;
				if ( list != null )
				{
					builder.Remove( o );
					builder.AddRange( Expand( Cast( list ) ) );
				}
			}
			var result = builder.ToImmutable();
			return result;
		}

		private static ImmutableArray<object> Cast(IList source)
		{
			var builder = ImmutableArray.CreateBuilder<object>();
			for ( var i = 0; i < source.Count; i++ )
			{
				builder.Add( source[i] );
			}
			var result = builder.ToImmutable();
			return result;
		}


		/*int GetCode( object key )
		{
			
			return result;
		}

		int CreateFrom( IEnumerable items )
		{
			var array = ImmutableArray.Create<object>( items );
			var result = Create( array.ToImmutable() );
			return result;
		}*/

		/*readonly CacheContext context = new CacheContext();

		class CacheContext
		{
			readonly ISet<object> keys = new HashSet<object>();
			readonly IDictionary<object, int> dictionary = new Dictionary<object, int>();

			public int Get( object item )
			{
				if ( !keys.Contains( item ) )
				{
					keys.Add( item );
					dictionary[item] = item.GetHashCode();
				}

				return dictionary[item];
			}
		}*/
	}
}