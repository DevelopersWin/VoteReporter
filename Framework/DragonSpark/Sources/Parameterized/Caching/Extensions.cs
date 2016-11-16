using DragonSpark.Extensions;
using System;

namespace DragonSpark.Sources.Parameterized.Caching
{
	public static class Extensions
	{
		public static ICache<T> ToCache<T>( this IParameterizedSource<object, T> @this ) => @this.ToDelegate().ToCache();
		public static ICache<T> ToCache<T>( this Func<object, T> @this ) => ParameterizedSources<T>.Default.Get( @this );
		sealed class ParameterizedSources<T> : Cache<Func<object, T>, ICache<T>>
		{
			public static ParameterizedSources<T> Default { get; } = new ParameterizedSources<T>();
			ParameterizedSources() : base( Caches.Create ) {}
		}

		public static ICache<TParameter, TResult> ToCache<TParameter, TResult>( this IParameterizedSource<TParameter, TResult> @this ) => @this.ToDelegate().ToCache();
		public static ICache<TParameter, TResult> ToCache<TParameter, TResult>( this Func<TParameter, TResult> @this ) => ParameterizedSources<TParameter, TResult>.Default.Get( @this );
		sealed class ParameterizedSources<TParameter, TResult> : Cache<Func<TParameter, TResult>, ICache<TParameter, TResult>>
		{
			public static ParameterizedSources<TParameter, TResult> Default { get; } = new ParameterizedSources<TParameter, TResult>();
			ParameterizedSources() : base( Caches.Create ) {}
		}

		public static ICache<TParameter, TResult> ToEqualityCache<TParameter, TResult>( this IParameterizedSource<TParameter, TResult> @this ) where TParameter : class => @this.ToDelegate().ToEqualityCache();
		public static ICache<TParameter, TResult> ToEqualityCache<TParameter, TResult>( this Func<TParameter, TResult> @this ) where TParameter : class => new EqualityReferenceCache<TParameter, TResult>( @this );
		
		public static TValue GetAssigned<TInstance, TValue>( this ICache<TInstance, TValue> @this, TInstance instance )
		{
			var result = @this.Get( instance );
			if ( !result.IsAssigned() )
			{
				@this.Remove( instance );
			}
			return result;
		}

		/*public static TValue GetOrSet<TInstance, TValue>( this IAssignableReferenceSource<TInstance, TValue> @this, TInstance instance, Func<TValue> factory )
		{
			var current = @this.Get( instance );
			var result = current.IsAssigned() ? current : @this.SetValue( instance, factory() );
			return result;
		}*/

		public static TValue SetValue<TInstance, TValue>( this IAssignableReferenceSource<TInstance, TValue> @this, TInstance instance, TValue value = default(TValue) )
		{
			if ( value.IsAssigned() )
			{
				@this.Set( instance, value );
			}
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
	}
}