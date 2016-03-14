using DragonSpark.Runtime.Values;
using System;
using System.Linq;
using DragonSpark.Extensions;
using DragonSpark.Setup.Registration;
using PostSharp.Patterns.Contracts;

namespace DragonSpark.Activation.FactoryModel
{
	public interface IFactory
	{
		object Create();
	}

	public interface IFactory<out T> : IFactory
	{
		new T Create();
	}

	public interface ITransformer<T> : IFactory<T, T>
	{}

	public interface IFactory<in TParameter, out TResult> : IFactoryWithParameter
	{
		TResult Create( TParameter parameter );
	}

	public static class FactoryExtensions
	{
		class Delegate<T, U> : ConnectedValue<Func<T, U>>
		{
			public Delegate( IFactoryWithParameter instance ) : base( instance, typeof(Delegate<T, U>), () => new Func<object, object>( instance.Create ).Convert<T,U>() )
			{}
		}

		class Delegate<T> : ConnectedValue<Func<T>>
		{
			public Delegate( IFactory instance ) : base( instance, typeof(Delegate<T>), () => new Func<object>( instance.Create ).Convert<T>() )
			{}
		}

		public static Delegate Convert( [Required]this Func<object> @this, [Required]Type resultType ) => (Delegate)typeof(FactoryExtensions).InvokeGeneric( nameof(Convert), resultType.ToItem(), @this );

		public static Delegate Convert( [Required]this Func<object, object> @this, [Required]Type parameterType, [Required]Type resultType ) => (Delegate)typeof(FactoryExtensions).InvokeGeneric( nameof(Convert), parameterType.Append( resultType ).ToArray(), @this );

		public static Func<T> Convert<T>( this Func<object> @this ) => () => (T)@this();

		public static Func<T, U> Convert<T, U>( this Func<object, object> @this ) => arg => (U)@this( arg );

		public static Func<T> ToDelegate<T>( this IFactory<T> @this ) => new Delegate<T>( @this ).Item;

		public static Func<T, U> ToDelegate<T, U>( this IFactory<T, U> @this ) => new Delegate<T,U>( @this ).Item;
	}
}