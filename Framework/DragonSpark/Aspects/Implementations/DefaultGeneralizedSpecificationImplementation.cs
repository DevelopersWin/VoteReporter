using DragonSpark.Aspects.Adapters;
using DragonSpark.Sources;
using DragonSpark.Sources.Coercion;
using DragonSpark.Specifications;

namespace DragonSpark.Aspects.Implementations
{
	class DefaultGeneralizedSpecificationImplementation<T> : DelegatedAdapter<T, bool>, ISpecification<object>
	{
		public DefaultGeneralizedSpecificationImplementation( ISpecification<T> implementation ) 
			: base( 
				SourceCoercer<ICoercerAdapter>.Default.Get( implementation )?.To( DefaultCoercer ) ?? DefaultCoercer, 
				new Adapters.SpecificationAdapter<T>( implementation ).Get ) {}

		public bool IsSatisfiedBy( object parameter ) => Get( Coerce( parameter ) );
		
	}
}