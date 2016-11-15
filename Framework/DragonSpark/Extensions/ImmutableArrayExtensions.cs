using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace DragonSpark.Extensions
{
	public static class ImmutableArrayExtensions
	{
		public static IEnumerable<T> AsEnumerable<T>( this ImmutableArray<T> source ) => source.ToArray();

		public static int Count<T>( this ImmutableArray<T> source, Func<T, bool> predicate ) => source.ToArray().Count( predicate );

		public static IEnumerable<T> Distinct<T>( this ImmutableArray<T> first ) => first.ToArray().Distinct();

		public static IEnumerable<TResult> SelectMany<TSource, TResult>(this ImmutableArray<TSource> @this, Func<TSource, IEnumerable<TResult>> selector)
			=> @this.ToArray().SelectMany( selector );

		public static IOrderedEnumerable<TSource> OrderByDescending<TSource, TKey>( this ImmutableArray<TSource> source, Func<TSource, TKey> keySelector, IComparer<TKey> comparer )
			=> source.ToArray().OrderByDescending( keySelector, comparer );

		public static IEnumerable<T> Union<T>( this ImmutableArray<T> first, IEnumerable<T> second ) => first.ToArray().Union( second );

		public static IEnumerable<T> Except<T>( this IEnumerable<T> first, ImmutableArray<T> second ) => first.Except( second.AsEnumerable() );

		public static IEnumerable<T> Except<T>( this ImmutableArray<T> first, IEnumerable<T> second ) => first.ToArray().Except( second );

		public static IEnumerable<T> Concat<T>( this ImmutableArray<T> first, IEnumerable<T> second ) => first.ToArray().Concat( second );

		public static IEnumerable<T> Concat<T>( this IEnumerable<ImmutableArray<T>> sources ) => sources.Select( array => array.ToArray() ).Concat();
	}
}