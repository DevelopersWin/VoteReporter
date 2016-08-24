using DragonSpark.Extensions;
using DragonSpark.Sources.Parameterized;
using DragonSpark.Sources.Parameterized.Caching;
using DragonSpark.TypeSystem;
using System;
using System.Reflection;
using System.Runtime.InteropServices;

namespace DragonSpark.Sources
{
	public static class Factory
	{
		public static T Self<T>( this T @this ) => @this;

		public static Func<T> For<T>( [Optional]T @this ) => @this.IsAssigned() ? ( typeof(T).GetTypeInfo().IsValueType ? new Source<T>( @this ) : @this.Sourced() ).Get : Delegates<T>.Default;

		public static Func<T> ToFixedDelegate<T>( this ISource<T> @this ) => new Func<T>( @this.Get ).Fix();
		public static Func<T> Fix<T>( this Func<T> @this ) => FixedDelegateBuilder<T>.Default.Get( @this );
		public static Func<TParameter, TResult> Fix<TParameter, TResult>( this Func<TParameter, TResult> @this ) => CacheFactory.Create( @this ).Get;

		public static Func<object, T> Global<T>( this ISource<T> @this ) => @this.ToDelegate().Global();
		public static Func<object, T> Global<T>( this Func<T> @this ) => @this.Wrap().Fix();
		public static Func<object, Func<TParameter, TResult>> Global<TParameter, TResult>( this Func<TParameter, TResult> @this ) => new Cache<TParameter, TResult>( @this ).Get;

		sealed class Cache<TParameter, TResult> : FactoryCache<Func<TParameter, TResult>>
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