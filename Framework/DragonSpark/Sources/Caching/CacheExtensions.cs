using System;
using System.Collections.Immutable;
using System.Linq;
using DragonSpark.Extensions;
using DragonSpark.Runtime;
using DragonSpark.TypeSystem;

namespace DragonSpark.Sources.Caching
{
	public static class CacheExtensions
	{
		public static TValue SetValue<TInstance, TValue>( this IAssignableParameterizedSource<TInstance, TValue> @this, TInstance instance, TValue value )
		{
			@this.Set( instance, value );
			return value;
		}

		public static TValue SetOrClear<TInstance, TValue>( this ICache<TInstance, TValue> @this, TInstance instance, TValue value = default(TValue) )
		{
			if ( value.IsAssigned() )
			{
				@this.Set( instance, value );
			}
			else
			{
				@this.Remove( instance );
			}
			
			return value;
		}

		public static Assignment<T1, T2> Assignment<T1, T2>( this ICache<T1, T2> @this, T1 first, T2 second )  => new Assignment<T1, T2>( new CacheAssign<T1, T2>( @this ), Assignments.From( first ), new Value<T2>( second ) );

		public static ImmutableArray<TResult> GetMany<TParameter, TResult>( this ICache<TParameter, TResult> @this, ImmutableArray<TParameter> parameters, Func<TResult, bool> where = null ) =>
			parameters
				.Select( @this.ToDelegate() )
				.Where( @where ?? Where<TResult>.Assigned ).ToImmutableArray();
		

		public static Func<TInstance, TValue> ToDelegate<TInstance, TValue>( this ICache<TInstance, TValue> @this ) => DelegateCache<TInstance, TValue>.Default.Get( @this );
		class DelegateCache<TInstance, TValue> : Cache<ICache<TInstance, TValue>, Func<TInstance, TValue>>
		{
			public static DelegateCache<TInstance, TValue> Default { get; } = new DelegateCache<TInstance, TValue>();

			DelegateCache() : base( command => command.Get ) {}
		}

		

		/*public static TDelegate Apply<TContext, TDelegate>( this ICache<TDelegate, TContext> @this, TDelegate source, TContext context ) where TDelegate : class
		{
			@this.Set( source, context );
			var result = Invocation.Create( source );
			return result;
		}

		public static TContext Context<TContext, TDelegate>( this ICache<TDelegate, TContext> @this ) where TDelegate : class
		{
			var instance = Invocation.GetCurrent() as TDelegate;
			var result = instance != null ? @this.Get( instance ) : default(TContext);
			return result;
		}*/
	}
}