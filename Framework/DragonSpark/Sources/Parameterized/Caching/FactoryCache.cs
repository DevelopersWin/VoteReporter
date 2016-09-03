using DragonSpark.Specifications;

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
			var factory = specification == DefaultSpecification ? source : new SpecificationParameterizedSource<TInstance, TValue>( specification, source.Get );
			configuration.Assign( factory.ToSourceDelegate().Wrap() );
		}

		protected abstract TValue Create( TInstance parameter );
	}

	public abstract class FactoryCache<T> : FactoryCache<object, T>, ICache<T>
	{
		protected FactoryCache() : this( DefaultSpecification ) {}
		protected FactoryCache( ISpecification<object> specification ) : base( specification ) {}
	}
}