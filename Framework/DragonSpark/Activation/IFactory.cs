using DragonSpark.Extensions;
using DragonSpark.Runtime.Values;
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
		/*public static Action<T> Synchronized<T>( this Action<T> @this ) where T : class => parameter =>
																						   {
																							   lock ( parameter )
																							   {
																								   @this( parameter );
																							   }
																						   };*/

		class Delegate<T, U> : ConnectedStore<Func<T, U>>
		{
			public Delegate( IFactory<T, U> source ) : base( source, typeof(Delegate<T, U>), () => source.Create ) {}

			// public Delegate( IFactoryWithParameter instance ) : base( instance, typeof(Delegate<T, U>), () => new Func<object, object>( instance.Create ).Convert<T,U>() ) {}
		}

		class Delegate<T> : ConnectedStore<Func<T>>
		{
			public Delegate( IFactory<T> source ) : base( source, typeof(Delegate<T>), () => source.Create ) {}

			// public Delegate( IFactory instance ) : base( instance, typeof(Delegate<T>), () => new Func<object>( instance.Create ).Convert<T>() ) {}
		}

		public static T CreateUsing<T>( this IFactoryWithParameter @this, object parameter ) => (T)@this.Create( parameter );

		public static T Self<T>( [Required] this T @this ) => @this;

		public static Delegate Convert( [Required]this Func<object> @this, [Required]Type resultType ) => (Delegate)typeof(FactoryExtensions).InvokeGeneric( nameof(Convert), resultType.ToItem(), @this );

		public static Delegate Convert( [Required]this Func<object, object> @this, [Required]Type parameterType, [Required]Type resultType ) => (Delegate)typeof(FactoryExtensions).InvokeGeneric( nameof(Convert), parameterType.Append( resultType ).ToArray(), @this );

		public static Func<T> Convert<T>( this Func<object> @this ) => () => (T)@this();

		public static Func<T, U> Convert<T, U>( this Func<object, object> @this ) => arg => (U)@this( arg );

		public static Func<T> ToDelegate<T>( this IFactory<T> @this ) => new Delegate<T>( @this ).Value;

		public static Func<T, U> ToDelegate<T, U>( this IFactory<T, U> @this ) => new Delegate<T,U>( @this ).Value;

		public static Func<T, T> ToFactory<T>( this Action<T> action ) => action.ToFactory<T, T>();

		public static Func<T,U> ToFactory<T,U>( this Action<T> action ) => parameter =>
																   {
																	   action( parameter );
																	   return Default<U>.Item;
																   };

		// public static TResult[] CreateMany<TParameter, TResult>( this IFactory<TParameter, TResult> @this, IEnumerable<TParameter> parameters ) => CreateMany( @this, parameters, Where<TResult>.NotNull );

		public static TResult[] CreateMany<TParameter, TResult>( this IFactory<TParameter, TResult> @this, IEnumerable<TParameter> parameters, Func<TResult, bool> where = null ) =>
			@this.CreateMany( parameters.Cast<object>(), where );
			/*parameters
				.Select( @this.Create )
				.Where( where )
				.Fixed();*/

		public static TResult[] CreateMany<TResult>( this IFactoryWithParameter @this, IEnumerable<object> parameters, Func<TResult, bool> where = null ) => 
			parameters
				.Select( @this.Create )
				.Cast<TResult>()
				.Where( where ?? Where<TResult>.NotNull )
				.Fixed();
	}
}