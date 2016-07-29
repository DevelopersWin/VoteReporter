using DragonSpark.Activation;
using DragonSpark.Runtime.Properties;
using System;

namespace DragonSpark.Runtime
{
	public interface ISource<out T> : ISource
	{
		new T Get();
	}

	public interface ISource
	{
		object Get();
	}

	public static class Source
	{
		public static Func<T> For<T>( this T @this ) where T : struct => @this.Sourced().Get;

		public static T Self<T>( this T @this ) => @this;

		public static ISource<T> Sourced<T>( this T @this ) => Sources<T>.Default.Get( @this );

		class Sources<T> : DecoratedCache<T, ISource<T>>
		{
			public static Sources<T> Default { get; } = new Sources<T>();
			Sources() {}
		}
	}

	public class Source<T> : ISource<T>
	{
		readonly T instance;

		public Source( T instance )
		{
			this.instance = instance;
		}

		public T Get() => instance;

		object ISource.Get() => Get();
	}

	public static class SourceExtensions
	{
		public static Func<TParameter, TResult> Delegate<TParameter, TResult>( this ISource<IParameterizedSource<TParameter, TResult>> @this ) => SourceDelegates<TParameter, TResult>.Default.Get( @this );
		class SourceDelegates<TParameter, TResult> : Cache<ISource<IParameterizedSource<TParameter, TResult>>, Func<TParameter, TResult>>
		{
			public static SourceDelegates<TParameter, TResult> Default { get; } = new SourceDelegates<TParameter, TResult>();
			SourceDelegates() : base( source => new Factory( source ).Create ) {}

			class Factory : FactoryBase<TParameter, TResult>
			{
				readonly ISource<IParameterizedSource<TParameter, TResult>> source;
				public Factory( ISource<IParameterizedSource<TParameter, TResult>> source )
				{
					this.source = source;
				}

				public override TResult Create( TParameter parameter ) => source.Get().Get( parameter );
			}
		}

		// public static T Self<T>( this T @this ) => @this;

		public static Func<T> Delegate<T>( this Func<ISource<T>> @this ) => Delegates<T>.Default.Get( @this );
		class Delegates<T> : Cache<Func<ISource<T>>, Func<T>>
		{
			public static Delegates<T> Default { get; } = new Delegates<T>();
			Delegates() : base( source => new Factory( source ).Create ) {}

			class Factory : FactoryBase<T>
			{
				readonly Func<ISource<T>> source;
				public Factory( Func<ISource<T>> source )
				{
					this.source = source;
				}

				public override T Create() => source().Get();
			}
		}

		/*public static Func<T> Delegate<T>( this ISource<ISource<T>> @this ) => @this.ToDelegate().Delegate();
		class SourceDelegates<T> : Cache<ISource<ISource<T>>, Func<T>>
		{
			public static Delegates<T> Default { get; } = new Delegates<T>();
			SourceDelegates() : base( source => new Factory( source ).Create ) {}

			class Factory : FactoryBase<T>
			{
				readonly ISource<ISource<T>> source;
				public Factory( ISource<ISource<T>> source )
				{
					this.source = source;
				}

				public override T Create() => source.Get().Get();
			}
		}*/
	}

	public interface IParameterizedSource<out T> : IParameterizedSource<object, T> {}

	public interface IParameterizedSource<in TParameter, out TResult> : IParameterizedSource
	{
		TResult Get( TParameter parameter );
	}

	public interface IParameterizedSource
	{
		object Get( object parameter );
	}

	public class ParameterizedConfiguration<TParameter, TResult> : ExecutionScope<ICache<TParameter, TResult>>, IParameterizedSource<TParameter, TResult>
	{
		public ParameterizedConfiguration( Func<TParameter, TResult> factory ) : base( new FixedFactory<Func<TParameter, TResult>, ICache<TParameter, TResult>>( CacheFactory.Create, factory ).Create ) {}

		public TResult Get( TParameter key ) => Value.Get( key );

		object IParameterizedSource.Get( object parameter ) => parameter is TParameter ? Get( (TParameter)parameter ) : default(TResult);
	}
}
