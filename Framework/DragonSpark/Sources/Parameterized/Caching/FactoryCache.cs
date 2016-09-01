using DragonSpark.Specifications;
using System;

namespace DragonSpark.Sources.Parameterized.Caching
{
	public abstract class FactoryCache<TInstance, TValue> : DecoratedCache<TInstance, TValue>
	{
		readonly protected static ISpecification<TInstance> DefaultSpecification = Specifications<TInstance>.Always;

		protected FactoryCache() : this( DefaultSpecification ) {}
		protected FactoryCache( ISpecification<TInstance> specification ) : this( new ParameterizedScope<TInstance, TValue>( instance => default(TValue) ), specification ) {}

		FactoryCache( IParameterizedScope<TInstance, TValue> configuration, ISpecification<TInstance> specification ) : base( configuration.ToCache() )
		{
			IParameterizedSource<TInstance, TValue> source = new DelegatedParameterizedSource<TInstance, TValue>( Create );
			var factory = specification == DefaultSpecification ? source : source.With( specification ).WithAutoValidation();
			configuration.Assign( new Func<TInstance, TValue>( factory.Get ).Wrap() );
		}

		protected abstract TValue Create( TInstance parameter );
	}

	public abstract class FactoryCache<T> : FactoryCache<object, T>, ICache<T>
	{
		protected FactoryCache() : this( DefaultSpecification ) {}
		protected FactoryCache( ISpecification<object> specification ) : base( specification ) {}
	}
}