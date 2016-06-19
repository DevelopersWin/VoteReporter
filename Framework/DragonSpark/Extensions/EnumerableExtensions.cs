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
		public static IEnumerable<T> WhereNot<T>( [Required] this IEnumerable<T> @this, [Required] Func<T, bool> where ) => @this.Where( where.Inverse() );

		// public static IEnumerable<T> OrItem<T>( this IEnumerable<T> @this, Func<T> defaultFunction ) where T : class => AnyOr( @this, defaultFunction().ToItem );

		public static IEnumerable<T> AnyOr<T>( this IEnumerable<T> @this, [Required]Func<IEnumerable<T>> defaultFunction ) => @this.With( x => x.Any() ) ? @this : defaultFunction();
		
		public static T[] Fixed<T>( this IEnumerable<T> @this )
		{
			var array = @this as T[] ?? @this.ToArray();
			var result = array.Length > 0 ? array : (T[])Enumerable.Empty<T>();
			return result;
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


		/*public static IEnumerable<T> Concat<T>(this IEnumerable<T> source, T value) => source.ConcatWorker(value);

		static IEnumerable<T> ConcatWorker<T>(this IEnumerable<T> source, T value)
		{
			foreach (var v in source)
			{
				yield return v;
			}

			yield return value;
		}

		public static bool SetEquals<T>(this IEnumerable<T> source1, IEnumerable<T> source2, IEqualityComparer<T> comparer) => source1.ToSet(comparer).SetEquals(source2);

		public static bool SetEquals<T>(this IEnumerable<T> source1, IEnumerable<T> source2) => source1.ToSet().SetEquals(source2);

		public static ISet<T> ToSet<T>(this IEnumerable<T> source, IEqualityComparer<T> comparer) => ImmutableHashSet.CreateRange( comparer, source );

		public static ISet<T> ToSet<T>(this IEnumerable<T> source) => source as ISet<T> ?? ImmutableHashSet.CreateRange( source );*/

		// public static IEnumerable<T> NullIfEmpty<T>( this IEnumerable<T> @this ) => @this.With( x => x.Any() ) ? @this : null;

		// public static IEnumerable<T> Prioritize<T>( this IEnumerable<T> @this, Func<T, IPriorityAware> determine ) => @this.Prioritize( x => determine( x ).Priority );

		public static IEnumerable<T> Prioritize<T>( [Required]this IEnumerable<T> @this ) => @this.OrderBy( PriorityAwareLocator<T>.Instance.ToDelegate(), PriorityComparer.Instance );

		public static U WithFirst<T, U>( this IEnumerable<T> @this, Func<T, U> with, Func<U> defaultFunction = null ) => WithFirst( @this, Where<T>.Always, with, defaultFunction );

		public static U WithFirst<T, U>( this IEnumerable<T> @this, Func<T, bool> where, Func<T, U> with, Func<U> defaultFunction = null ) => @this.Assigned().FirstOrDefault( @where ).With( with, defaultFunction );

		public static T Only<T>( this IEnumerable<T> @this ) => Only( @this, Where<T>.Always );

		public static T Only<T>( this IEnumerable<T> @this, Func<T, bool> where )
		{
			var enumerable = @this.Where( where ).ToImmutableArray();
			var result = enumerable.Length == 1 ? enumerable.Single() : default(T);
			return result;
		}

		public static void Each<T>( this IEnumerable<T> @this, Action<T> action ) => @this.ForEach( action );

		public static IEnumerable<TResult> Each<T, TResult>( this IEnumerable<T> enumerable, Func<T, TResult> action ) => enumerable.Select( action ).ToArray();

		public class Array<T> : Cache<T, T[]> where T : class
		{
			public static Array<T> Property { get; } = new Array<T>();

			Array() : base( arg => new[] { arg } ) {}
		}

		public static TItem[] ToItem<TItem>( this TItem target ) where TItem : class => Array<TItem>.Property.Get( target );

		public static IEnumerable<TItem> Append<TItem>( this TItem target, IEnumerable<TItem> second ) where TItem : class => target.Append( second.Fixed() );
		public static IEnumerable<TItem> Append<TItem>( this TItem target, params TItem[] second ) where TItem : class => target.ToItem().Concat( second );

		public static IEnumerable<TItem> Prepend<TItem>( this TItem target, IEnumerable<TItem> second ) where TItem : class=> target.Prepend( second.Fixed() );
		public static IEnumerable<TItem> Prepend<TItem>( this TItem target, params TItem[] second ) where TItem : class => second.Concat( target.ToItem() );

		public static IEnumerable<TItem> Append<TItem>( this IEnumerable<TItem> target, params TItem[] items ) => target.Concat( items );

		public static IEnumerable<TItem> Prepend<TItem>( this IEnumerable<TItem> target, params TItem[] items ) => items.Concat( target );

		public static IEnumerable<Tuple<T1, T2>> Tuple<T1, T2>( this IEnumerable<T1> target, IEnumerable<T2> other ) => target.Zip( other, System.Tuple.Create ).ToArray();

		public static T FirstAssigned<T>( this IEnumerable<T> @this ) => @this.Assigned().FirstOrDefault();

		public static U FirstAssigned<T, U>( this IEnumerable<T> @this, Func<T, U> projection ) => @this.Assigned().Select( projection ).FirstAssigned();

		public static IEnumerable<T> Assigned<T>( this IEnumerable<T> target ) => target.Where( Where<T>.Assigned );

		//public static IEnumerable<TItem> Assigned<TItem, TProject>( this IEnumerable<TItem> target, Func<TItem, TProject> project ) => target.Where(  );

		public static T FirstOrDefaultOfType<T>(this IEnumerable enumerable) => enumerable.OfType<T>().FirstOrDefault();

		public static T PeekOrDefault<T>( this System.Collections.Generic.Stack<T> @this ) => @this.Any() ? @this.Peek() : default(T);
	}
}