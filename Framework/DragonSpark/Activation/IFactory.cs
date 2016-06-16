using DragonSpark.Extensions;
using DragonSpark.Runtime.Properties;
using DragonSpark.Runtime.Specifications;
using DragonSpark.TypeSystem;
using PostSharp.Patterns.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DragonSpark.Activation
{
	public interface IFactory : ICreator
	{
		object Create();
	}

	public interface IFactory<out T> : IFactory
	{
		new T Create();
	}
	public interface ITransformer<T> : IFactory<T, T> {}

	public interface IFactory<in TParameter, out TResult> : IFactoryWithParameter
	{
		bool CanCreate( TParameter parameter );

		TResult Create( TParameter parameter );
	}

	

	public static class FactoryExtensions
	{
		public static T CreateUsing<T>( this IFactoryWithParameter @this, object parameter ) => (T)@this.Create( parameter );

		public static IFactory<object, T> Wrap<T>( this T @this ) where T : class => @this.Wrap<object, T>();

		public static IFactory<TParameter, TResult> Wrap<TParameter, TResult>( this TResult @this ) where TResult : class => InstanceFactory<TParameter, TResult>.Instance.Default.Get( @this );

		public static IFactory<object, T> Wrap<T>( this IFactory<T> @this ) => @this.Wrap<object, T>();

		public static IFactory<TParameter, TResult> Wrap<TParameter, TResult>( this IFactory<TResult> @this ) => @this.ToDelegate().Wrap<TParameter, TResult>();

		public static IFactory<object, T> Wrap<T>( this Func<T> @this ) => @this.Wrap<object, T>();

		public static IFactory<TParameter, TResult> Wrap<TParameter, TResult>( this Func<TResult> @this ) => WrappedFactory<TParameter, TResult>.FactoryInstance.Default.Get( @this );

		public static IFactory<T> ToFactory<T>( this T @this ) where T : class => Instance<T>.Default.Get( @this );

		public static IFactory<TParameter, TResult> ToFactory<TParameter, TResult>( this TResult @this ) where TResult : class => Instance<TResult>.Default.Get( @this ).Wrap<TParameter, TResult>();

		public static T Self<T>( [Required] this T @this ) => @this;

		public static Delegate Convert( [Required]this Func<object> @this, [Required]Type resultType ) => typeof(FactoryExtensions).Adapt().Invoke<Delegate>( nameof(Convert), resultType.ToItem(), @this );

		public static Delegate Convert( [Required]this Func<object, object> @this, [Required]Type parameterType, [Required]Type resultType ) => typeof(FactoryExtensions).Adapt().Invoke<Delegate>( nameof(Convert), parameterType.Append( resultType ).ToArray(), @this );

		public static Func<T> Convert<T>( this Func<object> @this ) => Converter<object, T>.Delegate.Default.Get( @this );

		public static IFactory<TParameter, TResult> Cast<TParameter, TResult>( this IFactoryWithParameter @this ) => @this as IFactory<TParameter, TResult> ?? Casted<TParameter, TResult>.Default.Get( @this );

		public static Func<TParameter, TResult> Convert<TParameter, TResult>( this Func<object, object> @this ) => Converter<object, object, TParameter, TResult>.Delegate.Default.Get( @this );

		public static Func<T> ToDelegate<T>( this IFactory<T> @this ) => FixedFactory<T>.Delegate.Default.Get( @this );

		public static Func<TParameter, TResult> ToDelegate<TParameter, TResult>( this IFactory<TParameter, TResult> @this ) => WrappedFactory<TParameter, TResult>.Delegate.Default.Get( @this );

		public static TResult[] CreateMany<TParameter, TResult>( this IFactory<TParameter, TResult> @this, IEnumerable<TParameter> parameters, Func<TResult, bool> where = null ) =>
			parameters
				.Where( @this.CanCreate )
				.Select( @this.Create )
				.Where( @where ?? Where<TResult>.NotNull ).Fixed();
		public static TResult[] CreateMany<TResult>( this IFactoryWithParameter @this, IEnumerable<object> parameters, Func<TResult, bool> where = null ) => 
			parameters
				.Where( @this.CanCreate )
				.Select( @this.Create )
				.Cast<TResult>()
				.Where( @where ?? Where<TResult>.NotNull ).Fixed();

		public static ICache<TParameter, TResult> Cached<TParameter, TResult>( this IFactory<TParameter, TResult> @this ) where TParameter : class where TResult : class => FactoryCache<TParameter, TResult>.Default.Get( @this );

		class FactoryCache<TParameter, TResult> : Cache<IFactory<TParameter, TResult>, ICache<TParameter, TResult>> where TParameter : class where TResult : class
		{
			public static FactoryCache<TParameter, TResult> Default { get; } = new FactoryCache<TParameter, TResult>();

			public FactoryCache() : base( factory => new Cache<TParameter, TResult>( factory.ToDelegate() ) ) {}
		}

		class Instance<T> : Cache<T, IFactory<T>> where T : class
		{
			public static Instance<T> Default { get; } = new Instance<T>();
			
			Instance() : base( result => new FixedFactory<T>( result ) ) {}
		}

		public class InstanceFactory<TParameter, TResult> : WrappedFactory<TParameter, TResult> where TResult : class
		{
			public InstanceFactory( TResult instance ) : base( instance.ToFactory().ToDelegate() ) {}

			public class Instance : Cache<TResult, IFactory<TParameter, TResult>>
			{
				public static Instance Default { get; } = new Instance();
			
				Instance() : base( result => new InstanceFactory<TParameter, TResult>( result ) ) {}
			}
		}

		class Casted<TParameter, TResult> : Cache<IFactoryWithParameter, IFactory<TParameter, TResult>>
		{
			public static Casted<TParameter, TResult> Default { get; } = new Casted<TParameter, TResult>();
			
			Casted() : base( result => new CastedFactory<TParameter, TResult>( result ) ) {}
		}

		public class CastedFactory<TParameter, TResult> : FactoryBase<TParameter, TResult>
		{
			readonly IFactoryWithParameter inner;
			public CastedFactory( IFactoryWithParameter inner ) : base( new DelegatedSpecification<object>( inner.CanCreate ).Cast<TParameter>() )
			{
				this.inner = inner;
			}

			public override TResult Create( TParameter parameter ) => (TResult)inner.Create( parameter );
		}

		class Converter<TFrom, TTo> where TTo : TFrom where TFrom : class
		{
			readonly Func<TFrom> from;

			Converter( Func<TFrom> from )
			{
				this.from = from;
			}

			TTo To() => (TTo)from();

			public class Delegate : Cache<Func<TFrom>, Func<TTo>>
			{
				public static Delegate Default { get; } = new Delegate();

				Delegate() : base( result => new Converter<TFrom, TTo>( result ).To ) {}
			}
		}

		class Converter<TFromParameter, TFromResult, TToParameter, TToResult> where TToResult : TFromResult where TToParameter : TFromParameter
		{
			readonly Func<TFromParameter, TFromResult> from;

			Converter( Func<TFromParameter, TFromResult> from )
			{
				this.from = from;
			}

			TToResult To( TToParameter parameter ) => (TToResult)from( parameter );

			public class Delegate : Cache<Func<TFromParameter, TFromResult>, Func<TToParameter, TToResult>>
			{
				public static Delegate Default { get; } = new Delegate();

				Delegate() : base( result => new Converter<TFromParameter, TFromResult, TToParameter, TToResult>( result ).To ) {}
			}

		}
	}
}