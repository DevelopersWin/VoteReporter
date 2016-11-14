using DragonSpark.Commands;
using DragonSpark.Runtime.Assignments;
using System;
using System.Collections.Generic;

namespace DragonSpark.Extensions
{
	public static class DictionaryExtensions
	{
		public static IEnumerable<TValue> Yield<TKey,TValue>( this IDictionary<TKey,TValue> target, TKey key ) => target.TryGet( key )?.Yield();
		public static TValue TryGet<TKey,TValue>( this IDictionary<TKey,TValue> target, TKey key ) => TryGet( target, key, () => default(TValue) );

		public static TValue TryGet<TKey,TValue>( this IDictionary<TKey,TValue> target, TKey key, Func<TValue> defaultValue ) => key.IsAssigned() && target.ContainsKey( key ) ? target[ key ] : defaultValue.With( x => x() );

		public static TValue Ensure<TKey, TValue>( this IDictionary<TKey, TValue> target, TKey key, Func<TKey,TValue> resolve )
		{
			if ( !target.ContainsKey( key ) )
			{
				target.Add( key, resolve( key ) );
			}
			return target[ key ];
		}

		public static IDisposable Assignment<TKey, TValue>( this IDictionary<TKey, TValue> @this, TKey first, TValue second )  => new Assignment<TKey, TValue>( new DictionaryAssign<TKey, TValue>( @this ), Assignments.From( first ), new Value<TValue>( second ) ).AsExecuted();

		public static void Set<TKey, TValue>( this IDictionary<TKey, TValue> @this, TKey instance, TValue value = default(TValue) ) => @this[instance] = value;

		public static bool Assign<TKey, TValue>( this IDictionary<TKey, TValue> @this, TKey instance, TValue value = default(TValue) )
		{
			var result = !@this.ContainsKey( instance ) && value.IsAssigned();
			if ( result )
			{
				@this.Set( instance, value );
			}
			return result;
		}

		public static TValue SetOrClear<TKey, TValue>( this IDictionary<TKey, TValue> @this, TKey instance, TValue value = default(TValue) )
		{
			if ( !@this.Assign( instance, value ) )
			{
				@this.Remove( instance );
			}
			return value;
		}
	}
}