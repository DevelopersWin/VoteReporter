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

		public static TResult Call<TParameter, TResult>( this Func<TResult> @this, TParameter _ ) => @this();
		public static TResult Call<TParameter, TResult>( this ISource<TResult> @this, TParameter _ ) => @this.ToDelegate().Call( _ );


		public static Func<TParameter, TResult> ToDelegate<TParameter, TResult>( this IParameterizedSource<TParameter, TResult> @this, object _ ) => @this.ToDelegate();
		
		public static Func<T> Enclose<T>( this T @this ) => ( typeof(T).GetTypeInfo().IsValueType ? new Source<T>( @this ) : @this.Sourced() ).Get;
	}
}