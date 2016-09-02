using DragonSpark.Aspects.Validation;
using DragonSpark.Specifications;

namespace DragonSpark.Sources.Parameterized
{
	class AutoValidatingSource<TParameter, TResult> : AutoValidatingSourceBase<TParameter, TResult>, IValidatedParameterizedSource<TParameter, TResult>
	{
		readonly ISpecification<object> specification;
		readonly IParameterizedSource<object, TResult> inner;

		public AutoValidatingSource( IValidatedParameterizedSource<TParameter, TResult> inner ) : this( inner, inner ) {}

		public AutoValidatingSource( ISpecification<TParameter> specification, IParameterizedSource<TParameter, TResult> inner ) : 
			base( new AutoValidationController( new FactoryAdapter<TParameter, TResult>( specification ) ), specification.IsSatisfiedBy, inner.Get )
		{
			this.specification = specification.With( Coercer<TParameter>.Default );
			this.inner = inner.With( Coercer<TParameter>.Default );
		}

		public object Get( object parameter ) => inner.Get( parameter );
		
		public bool IsSatisfiedBy( object parameter ) => specification.IsSatisfiedBy( parameter );
	}
}