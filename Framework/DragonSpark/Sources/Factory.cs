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
		public static TResult Accept<TParameter, TResult>( this ISource<TResult> @this, TParameter _ ) => @this.Get();

		public static Func<T> Enclose<T>( this T @this ) => ( typeof(T).GetTypeInfo().IsValueType ? new Source<T>( @this ) : @this.Sourced() ).Get;

		public static Func<TParameter, TResult> Enclose<TParameter, TResult>( this TResult @this ) => @this.Enclose().Accept;
	}
}