using DragonSpark.Activation;
using DragonSpark.Aspects;
using DragonSpark.Runtime.Properties;
using DragonSpark.TypeSystem;
using PostSharp.Patterns.Contracts;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace DragonSpark.Extensions
{
	public static class EnumerableExtensions
	{
		public static T[] Fixed<T>( this IEnumerable<T> @this )
		{
			var array = @this as T[] ?? @this.ToArray();
			var result = array.Length > 0 ? array : (T[])Enumerable.Empty<T>();
			return result;
		}


		public static IEnumerable<ValueTuple<T1, T2>> Introduce<T1, T2>( this ImmutableArray<T1> @this, T2 instance ) => Introduce( @this, instance, Where<ValueTuple<T1, T2>>.Always, Delegates<ValueTuple<T1, T2>>.Self );

		public static IEnumerable<T1> Introduce<T1, T2>( this ImmutableArray<T1> @this, T2 instance, Func<ValueTuple<T1, T2>, bool> where ) => Introduce( @this, instance, @where, tuple => tuple.Item1 );

		public static IEnumerable<TResult> Introduce<T1, T2, TResult>( this ImmutableArray<T1> @this, T2 instance, Func<ValueTuple<T1, T2>, TResult> select ) => Introduce( @this, instance, Where<ValueTuple<T1, T2>>.Always, @select );

		public static IEnumerable<TResult> Introduce<T1, T2, TResult>( this ImmutableArray<T1> @this, T2 instance, Func<ValueTuple<T1, T2>, bool> where, Func<ValueTuple<T1, T2>, TResult> select )
		{
			foreach ( var item in @this )
			{
				var tuple = ValueTuple.Create( item, instance );
				if ( where( tuple ) )
				{
					yield return select( tuple );
				}
			}
		}

		public static IEnumerable<ValueTuple<T1, T2>> Introduce<T1, T2>( this IEnumerable<T1> @this, T2 instance ) => Introduce( @this, instance, Where<ValueTuple<T1, T2>>.Always, Delegates<ValueTuple<T1, T2>>.Self );

		public static IEnumerable<T1> Introduce<T1, T2>( this IEnumerable<T1> @this, T2 instance, Func<ValueTuple<T1, T2>, bool> where ) => Introduce( @this, instance, @where, tuple => tuple.Item1 );

		public static IEnumerable<TResult> Introduce<T1, T2, TResult>( this IEnumerable<T1> @this, T2 instance, Func<ValueTuple<T1, T2>, TResult> select ) => Introduce( @this, instance, Where<ValueTuple<T1, T2>>.Always, @select );

		public static IEnumerable<TResult> Introduce<T1, T2, TResult>( this IEnumerable<T1> @this, T2 instance, Func<ValueTuple<T1, T2>, bool> where, Func<ValueTuple<T1, T2>, TResult> select )
		{
			foreach ( var item in @this )
			{
				var tuple = ValueTuple.Create( item, instance );
				if ( where( tuple ) )
				{
					yield return select( tuple );
				}
			}
		}

		/*public struct Enumerator<T>
		{
			readonly IList<T> pool;
			int index;

			public Enumerator( IList<T> pool )
			{
				this.pool = pool;
				index = 0;
			}

			public T Current
			{
				get
				{
					if ( pool == null || index == 0 )
						throw new InvalidOperationException();

					return pool[index - 1];
				}
			}

			public bool MoveNext()
			{
				index++;
				return pool != null && pool.Count >= index;
			}

			public void Reset() => index = 0;
		}

		public struct Enumerable<T>
		{
			readonly IList<T> pool;

			public Enumerable( IList<T> pool )
			{
				this.pool = pool;
			}

			public Enumerator<T> GetEnumerator() => new Enumerator<T>( pool );
		}*/


		public static bool All(this IEnumerable<bool> source)
		{
			foreach (var b in source)
			{
				if (!b)
				{
					return false;
				}
			}

			return true;
		}

		public static IEnumerable<T> Prioritize<T>( [Required]this IEnumerable<T> @this ) => @this.OrderBy( PriorityAwareLocator<T>.Instance.ToDelegate(), PriorityComparer.Instance );

		public static U WithFirst<T, U>( this IEnumerable<T> @this, Func<T, U> with, Func<U> defaultFunction = null ) => WithFirst( @this, Where<T>.Always, with, defaultFunction );

		public static U WithFirst<T, U>( this IEnumerable<T> @this, Func<T, bool> where, Func<T, U> with, Func<U> defaultFunction = null ) => @this.WhereAssigned().FirstOrDefault( @where ).With( with, defaultFunction );

		public static T Only<T>( this IEnumerable<T> @this ) => Only( @this, Where<T>.Always );

		public static T Only<T>( this IEnumerable<T> @this, Func<T, bool> where )
		{
			var enumerable = @this.Where( where ).ToArray();
			var result = enumerable.Length == 1 ? enumerable[0] : default(T);
			return result;
		}

		public static void Each<T>( this IEnumerable<T> @this, Action<T> action ) => @this.ForEach( action );

		public static IEnumerable<TResult> Each<T, TResult>( this IEnumerable<T> enumerable, Func<T, TResult> action ) => enumerable.Select( action ).ToArray();

		public class Array<T> : Cache<T, T[]> where T : class
		{
			public static Array<T> Default { get; } = new Array<T>();

			Array() : base( arg => new[] { arg } ) {}
		}

		public static TItem[] ToItem<TItem>( this TItem target ) where TItem : class => Array<TItem>.Default.Get( target );

		public static IEnumerable<T> Append<T>( this T @this, params T[] second ) => @this.Append_( second );
		public static IEnumerable<T> Append<T>( this T @this, IEnumerable<T> second ) => @this.Append_( second );
		static IEnumerable<T> Append_<T>( this T @this, IEnumerable<T> second )
		{
			yield return @this;
			foreach ( var element1 in second )
				yield return element1;
		}

		public static IEnumerable<T> Append<T>( this IEnumerable<T> @this, params T[] items ) => @this.Concat( items );

		public static IEnumerable<T> Append<T>( this IEnumerable<T> collection, T element )
		{
			foreach ( var element1 in collection )
				yield return element1;
			yield return element;
		}

		public static IEnumerable<T> Prepend<T>( this T @this, params T[] second ) => @this.Prepend_( second );
		public static IEnumerable<T> Prepend<T>( this T @this, IEnumerable<T> second ) => @this.Prepend_( second );
		static IEnumerable<T> Prepend_<T>( this T @this, IEnumerable<T> second )
		{
			foreach ( var item in second )
				yield return item;
			yield return @this;
		}

		public static IEnumerable<T> Prepend<T>( this IEnumerable<T> @this, params T[] items ) => items.Concat( @this );

		public static IEnumerable<ValueTuple<T1, T2>> Tuple<T1, T2>( this IEnumerable<T1> target, IEnumerable<T2> other ) => target.Zip( other, ValueTuple.Create ).ToArray();

		public static T FirstAssigned<T>( this IEnumerable<T> @this ) => @this.WhereAssigned().FirstOrDefault();

		public static TTo FirstAssigned<TFrom, TTo>( this IEnumerable<TFrom> @this, Func<TFrom, TTo> projection ) => @this.WhereAssigned().Select( projection ).FirstAssigned();

		public static IEnumerable<T> WhereAssigned<T>( this IEnumerable<T> target ) => target.Where( Where<T>.Assigned );

		public static T FirstOrDefaultOfType<T>(this IEnumerable enumerable) => enumerable.OfType<T>().FirstOrDefault();

		public static T PeekOrDefault<T>( this System.Collections.Generic.Stack<T> @this ) => @this.Any() ? @this.Peek() : default(T);
	}
}