using DragonSpark.Extensions;
using DragonSpark.Runtime.Properties;
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

		public static IFactory<object, TResult> Wrap<TResult>( this TResult @this ) where TResult : class => @this.Wrap<object, TResult>();

		public static IFactory<TParameter, TResult> Wrap<TParameter, TResult>( this TResult @this ) where TResult : class => InstanceFactory<TParameter, TResult>.Instance.Default.Get( @this );

		public static IFactory<object, TResult> Wrap<TResult>( this Func<TResult> @this ) => @this.Wrap<object, TResult>();

		public static IFactory<TParameter, TResult> Wrap<TParameter, TResult>( this Func<TResult> @this ) => Factory<TParameter, TResult>.FactoryInstance.Default.Get( @this );

		public static IFactory<T> ToFactory<T>( this T @this ) where T : class => Instance<T>.Default.Get( @this );

		public static T Self<T>( [Required] this T @this ) => @this;

		public static Delegate Convert( [Required]this Func<object> @this, [Required]Type resultType ) => typeof(FactoryExtensions).Adapt().Invoke<Delegate>( nameof(Convert), resultType.ToItem(), @this );

		public static Delegate Convert( [Required]this Func<object, object> @this, [Required]Type parameterType, [Required]Type resultType ) => typeof(FactoryExtensions).Adapt().Invoke<Delegate>( nameof(Convert), parameterType.Append( resultType ).ToArray(), @this );

		public static Func<T> Convert<T>( this Func<object> @this ) => Converter<object, T>.Delegate.Default.Get( @this );

		public static Func<TParameter, TResult> Convert<TParameter, TResult>( this Func<object, object> @this ) => Converter<object, object, TParameter, TResult>.Delegate.Default.Get( @this );

		public static Func<T> ToDelegate<T>( this IFactory<T> @this ) => Factory<T>.Delegate.Default.Get( @this );

		public static Func<TParameter, TResult> ToDelegate<TParameter, TResult>( this IFactory<TParameter, TResult> @this ) => Factory<TParameter, TResult>.Delegate.Default.Get( @this );

		/*public static Func<T, T> ToFactory<T>( this Action<T> action ) => action.ToFactory<T, T>();

		public static Func<T,U> ToFactory<T,U>( this Action<T> action ) => parameter =>
																   {
																	   action( parameter );
																	   return Default<U>.Item;
																   };*/

		// public static TResult[] CreateMany<TParameter, TResult>( this IFactory<TParameter, TResult> @this, IEnumerable<TParameter> parameters ) => CreateMany( @this, parameters, Where<TResult>.NotNull );

		public static TResult[] CreateMany<TParameter, TResult>( this IFactory<TParameter, TResult> @this, IEnumerable<TParameter> parameters, Func<TResult, bool> where = null ) =>
			EnumerableExtensions.Fixed( parameters
							.Where( @this.CanCreate )
							.Select( @this.Create )
							.Where( @where ?? Where<TResult>.NotNull ) );
			/*parameters
				.Select( @this.Create )
				.Where( where )
				.Fixed();*/

		public static TResult[] CreateMany<TResult>( this IFactoryWithParameter @this, IEnumerable<object> parameters, Func<TResult, bool> where = null ) => 
			EnumerableExtensions.Fixed( parameters
							.Where( @this.CanCreate )
							.Select( @this.Create )
							.Cast<TResult>()
							.Where( @where ?? Where<TResult>.NotNull ) );

		class Factory<T> : IFactory<T>
		{
			readonly T instance;

			public Factory( T instance )
			{
				this.instance = instance;
			}

			public T Create() => instance;

			object IFactory.Create() => Create();

			public class Delegate : AttachedProperty<IFactory<T>, Func<T>>
			{
				public static Delegate Default { get; } = new Delegate();

				Delegate() : base( factory => factory.Create ) {}
			}
		}

		class Instance<T> : AttachedProperty<T, IFactory<T>> where T : class
		{
			public static Instance<T> Default { get; } = new Instance<T>();
			
			Instance() : base( result => new Factory<T>( result ) ) {}
		}

		class InstanceFactory<TParameter, TResult> : Factory<TParameter, TResult> where TResult : class
		{
			InstanceFactory( TResult instance ) : base( instance.ToFactory().ToDelegate() ) {}

			public class Instance : AttachedProperty<TResult, IFactory<TParameter, TResult>>
			{
				public static Instance Default { get; } = new Instance();
			
				Instance() : base( result => new InstanceFactory<TParameter, TResult>( result ) ) {}
			}
		}

		class Factory<TParameter, TResult> : IFactory<TParameter, TResult>
		{
			readonly Func<TResult> item;

			protected Factory( Func<TResult> item )
			{
				this.item = item;
			}

			bool IFactoryWithParameter.CanCreate( object parameter ) => true;
			object IFactoryWithParameter.Create( object parameter ) => Create( default(TParameter) );
			bool IFactory<TParameter, TResult>.CanCreate( TParameter parameter ) => true;
			public TResult Create( TParameter parameter ) => item();

			public class Delegate : AttachedProperty<IFactory<TParameter, TResult>, Func<TParameter, TResult>>
			{
				public static Delegate Default { get; } = new Delegate();

				Delegate() : base( factory => factory.Create ) {}
			}

			public class FactoryInstance : AttachedProperty<Func<TResult>, Factory<TParameter, TResult>>
			{
				public static FactoryInstance Default { get; } = new FactoryInstance();
			
				FactoryInstance() : base( result => new Factory<TParameter, TResult>( result ) ) {}
			}
		}

		class Converter<TFrom, TTo> where TTo : TFrom where TFrom : class
		{
			readonly Func<TFrom> from;

			Converter( Func<TFrom> from )
			{
				this.from = from;
			}

			TTo To() => (TTo)from();

			public class Delegate : AttachedProperty<Func<TFrom>, Func<TTo>>
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

			public class Delegate : AttachedProperty<Func<TFromParameter, TFromResult>, Func<TToParameter, TToResult>>
			{
				public static Delegate Default { get; } = new Delegate();

				Delegate() : base( result => new Converter<TFromParameter, TFromResult, TToParameter, TToResult>( result ).To ) {}
			}

		}
	}
}