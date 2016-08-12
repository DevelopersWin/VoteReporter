using System;
using System.Reflection;
using DragonSpark.Activation.Sources.Caching;

namespace DragonSpark.Activation.Sources
{
	public static class Factory
	{
		public static T Self<T>( this T @this ) => @this;

		public static Func<T> For<T>( T @this ) => ( typeof(T).GetTypeInfo().IsValueType ? new Source<T>( @this ) : @this.Sourced() ).Get;

		public static Func<T> Fix<T>( this ISource<T> @this ) => new Func<T>( @this.Get ).Fix();
		public static Func<T> Fix<T>( this Func<T> @this ) => FixedDelegateBuilder<T>.Instance.Get( @this );
		public static Func<TParameter, TResult> Fix<TParameter, TResult>( this Func<TParameter, TResult> @this ) => CacheFactory.Create( @this ).Get;

		public static Func<object, T> Scope<T>( this Func<T> @this ) => @this.Wrap().Fix();

		public static Func<object, Func<TParameter, TResult>> CachedPerScope<TParameter, TResult>( this Func<TParameter, TResult> @this ) => new Cache<TParameter, TResult>( @this ).Get;
		class Cache<TParameter, TResult> : FactoryCache<Func<TParameter, TResult>>
		{
			readonly Func<TParameter, TResult> factory;

			public Cache( Func<TParameter, TResult> factory )
			{
				this.factory = factory;
			}

			protected override Func<TParameter, TResult> Create( object parameter ) => CacheFactory.Create( factory ).Get;
		}
	}
}