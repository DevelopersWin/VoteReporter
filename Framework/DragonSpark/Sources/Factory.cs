using DragonSpark.Sources.Parameterized;
using DragonSpark.Sources.Scopes;
using System;
using System.Reflection;

namespace DragonSpark.Sources
{
	public static class Factory
	{
		public static T Self<T>( this T @this ) where T : class => @this;

		public static TResult Accept<TParameter, TResult>( this TResult @this, TParameter _ ) => @this;

		public static TResult Accept<TParameter, TResult>( this Func<TResult> @this, TParameter _ ) => @this();
		public static T Fix<T>( this Func<T> @this, object _ ) => @this();
		public static T Fix<T>( this ISource<T> @this, object _ ) => @this.Get();

		public static TResult Fix<TParameter, TResult>( this ISource<TResult> @this, TParameter _ ) => @this.Get();

		// public static T Accept<T>( this ISource<T> @this, object _ ) => @this.Get();
		public static Func<TParameter, TResult> Allot<TParameter, TResult>( this IParameterizedSource<TParameter, TResult> @this, object _ ) => @this.ToDelegate();
		// public static Func<TParameter, TResult> Accept<TParameter, TResult>( this Func<TParameter, TResult> @this, object _ ) => @this;

		public static Func<T> Enclose<T>( this T @this ) => ( typeof(T).GetTypeInfo().IsValueType ? new Source<T>( @this ) : @this.Sourced() ).Get;
	}
}