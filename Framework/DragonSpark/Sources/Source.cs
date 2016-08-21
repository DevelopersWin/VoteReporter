using System;
using DragonSpark.Activation;
using DragonSpark.Extensions;
using DragonSpark.Sources.Parameterized.Caching;
using DragonSpark.TypeSystem;

namespace DragonSpark.Sources
{
	public static class Source
	{
		static IGenericMethodContext<Invoke> Methods { get; } = typeof(Source).Adapt().GenericFactoryMethods[nameof(Empty)];

		public static ISource Empty( Type type ) => Methods.Make( type ).Invoke<ISource>();

		public static ISource<T> Empty<T>() => EmptySource<T>.Default;

		public static ISource<T> Sourced<T>( this T @this ) => Support<T>.Sources.Get( @this );

		static class Support<T>
		{
			public static ICache<T, ISource<T>> Sources { get; } = CacheFactory.Create<T, ISource<T>>( arg => new Source<T>( arg ) );
		}

		/*public static IParameterizedSource<TParameter, TResult> ToFactory<TParameter, TResult>( this Func<TParameter, TResult> @this ) => ParameterizedSources<TParameter, TResult>.Default.Get( @this );
		// public static IFactory<TParameter, TResult> ToFactory<TParameter, TResult>( this TResult @this ) where TResult : class => ParameterizedSources<TResult>.Default.Get( @this ).Wrap<TParameter, TResult>();
		class ParameterizedSources<TParameter, TResult> : Cache<Func<TParameter, TResult>, IParameterizedSource<TParameter, TResult>>
		{
			public static ParameterizedSources<TParameter, TResult> Default { get; } = new ParameterizedSources<TParameter, TResult>();
			ParameterizedSources() : base( result => new DelegatedFactory<TParameter, TResult>( result ) ) {}
		}*/
	}
}