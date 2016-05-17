using DragonSpark.Runtime.Values;
using DragonSpark.TypeSystem;
using PostSharp.Patterns.Contracts;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DragonSpark.Extensions
{
	public static class EnumerableExtensions
	{
		public static IEnumerable<T> WhereNot<T>( [Required] this IEnumerable<T> @this, [Required] Func<T, bool> where ) => @this.Where( t => !where( t ) );

		public static IEnumerable<T> OrItem<T>( this IEnumerable<T> @this, Func<T> defaultFunction ) where T : class => AnyOr( @this, defaultFunction().ToItem );

		public static IEnumerable<T> AnyOr<T>( this IEnumerable<T> @this, [Required]Func<IEnumerable<T>> defaultFunction ) => @this.With( x => x.Any() ) ? @this : defaultFunction();
		
		public static T[] Fixed<T>( this IEnumerable<T> @this ) => @this as T[] ?? @this.With( x => x.ToArray().With( arg => arg.Any() ? arg : Default<T>.Items ) );

		public static IEnumerable<T> NullIfEmpty<T>( this IEnumerable<T> @this ) => @this.With( x => x.Any() ) ? @this : null;

		public static IEnumerable<T> Prioritize<T>( this IEnumerable<T> @this, Func<T, IPriorityAware> determine ) => @this.Prioritize( x => determine( x ).Priority );

		public static IEnumerable<T> Prioritize<T>( [Required]this IEnumerable<T> items, Func<T, Priority> determine = null ) => 
			items.OrderBy( determine ?? ( x => x.AsTo<IPriorityAware, Priority>( aware => aware.Priority, () => x.From<PriorityAttribute, Priority>( y => y.Priority, () => Priority.Normal ) ) ) );

		public static U WithFirst<T, U>( this IEnumerable<T> @this, Func<T, U> with, Func<U> defaultFunction = null ) => WithFirst( @this, t => true, with, defaultFunction );

		public static U WithFirst<T, U>( this IEnumerable<T> @this, Func<T, bool> where, Func<T, U> with, Func<U> defaultFunction = null ) => @this.NotNull().FirstOrDefault( @where ).With( with, defaultFunction );

		public static T Only<T>( this IEnumerable<T> @this, Func<T, bool> where = null )
		{
			var items = where.With( @this.Where, () => @this );
			var enumerable = items.Fixed();
			var result = enumerable.Length == 1 ? enumerable.Single() : default(T);
			return result;
		}

		public static void Each<T>( this IEnumerable<T> enumerable, Action<T> action ) => AutoMapper.Internal.EnumerableExtensions.Each( enumerable, action );

		public static IEnumerable<TResult> Each<T, TResult>( this IEnumerable<T> enumerable, Func<T, TResult> action ) => enumerable.Select( action ).ToArray();

		class Array<T> : ConnectedStore<T[]>
		{
			public Array( T instance ) : base( instance, typeof(Array<T>), () => new[] { instance } ) {}
		}

		public static TItem[] ToItem<TItem>( this TItem target ) => new Array<TItem>( target ).Value;

		public static IEnumerable<TItem> Append<TItem>( this TItem target, IEnumerable<TItem> second ) => target.Append( second.Fixed() );
		public static IEnumerable<TItem> Append<TItem>( this TItem target, params TItem[] second ) => target.ToItem().Concat( second );

		public static IEnumerable<TItem> Prepend<TItem>( this TItem target, IEnumerable<TItem> second ) => target.Prepend( second.Fixed() );
		public static IEnumerable<TItem> Prepend<TItem>( this TItem target, params TItem[] second ) => second.Concat( target.ToItem() );

		public static IEnumerable<TItem> Append<TItem>( this IEnumerable<TItem> target, params TItem[] items ) => target.Concat( items );

		public static IEnumerable<TItem> Prepend<TItem>( this IEnumerable<TItem> target, params TItem[] items ) => items.Concat( target );

		public static IEnumerable<Tuple<TFirst, TLast>> TupleWith<TFirst, TLast>( this IEnumerable<TFirst> target, IEnumerable<TLast> other ) => target.Select( ( first, i ) => new Tuple<TFirst, TLast>( first, other.ElementAtOrDefault( i ) ) ).ToArray();

		public static U FirstWhere<T, U>( this IEnumerable<T> @this, Func<T, U> where ) => @this.NotNull().Select( @where ).NotNull().FirstOrDefault();

		public static IEnumerable<TItem> NotNull<TItem>( this IEnumerable<TItem> target ) => NotNull( target, Default<TItem>.Self );

		public static IEnumerable<TItem> NotNull<TItem, TProject>( this IEnumerable<TItem> target, Func<TItem, TProject> project ) => target.Where( x => !project( x ).IsNull() );

		public static T FirstOrDefaultOfType<T>(this IEnumerable enumerable) => enumerable.OfType<T>().FirstOrDefault();

		public static T PeekOrDefault<T>( this Stack<T> @this ) => @this.Any() ? @this.Peek() : default(T);
	}
}