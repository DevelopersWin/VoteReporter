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

	public static class SourceExtensions
	{
		public static Func<TParameter, TResult> Delegate<TParameter, TResult>( this ISource<IParameterizedSource<TParameter, TResult>> @this ) => SourceDelegateCache<TParameter, TResult>.Default.Get( @this );
		class SourceDelegateCache<TParameter, TResult> : Cache<ISource<IParameterizedSource<TParameter, TResult>>, Func<TParameter, TResult>>
		{
			public static SourceDelegateCache<TParameter, TResult> Default { get; } = new SourceDelegateCache<TParameter, TResult>();
			SourceDelegateCache() : base( source => new Factory( source ).Create ) {}

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

		public static Func<T> Delegate<T>( this ISource<ISource<T>> @this ) => SourceDelegateCache<T>.Default.Get( @this );
		class SourceDelegateCache<T> : Cache<ISource<ISource<T>>, Func<T>>
		{
			public static SourceDelegateCache<T> Default { get; } = new SourceDelegateCache<T>();
			SourceDelegateCache() : base( source => new Factory( source ).Create ) {}

			class Factory : FactoryBase<T>
			{
				readonly ISource<ISource<T>> source;
				public Factory( ISource<ISource<T>> source )
				{
					this.source = source;
				}

				public override T Create() => source.Get().Get();
			}
		}
	}

	public interface IParameterizedSource<out T> : IParameterizedSource<object, T> {}

	public interface IParameterizedSource<in TParameter, out TResult>
	{
		TResult Get( TParameter parameter );
	}

	public class ParameterizedSource<TParameter, TResult> : ExecutionScope<ICache<TParameter, TResult>>, IParameterizedSource<TParameter, TResult>
	{
		public ParameterizedSource( Func<TParameter, TResult> factory ) : base( new Factory( factory ).Create ) {}

		public TResult Get( TParameter key ) => Value.Get( key );

		sealed class Factory : FactoryBase<ICache<TParameter, TResult>>
		{
			readonly Func<TParameter, TResult> factory;
			public Factory( Func<TParameter, TResult> factory )
			{
				this.factory = factory;
			}

			public override ICache<TParameter, TResult> Create() => CacheFactory.Create( factory );
		}
	}
}
