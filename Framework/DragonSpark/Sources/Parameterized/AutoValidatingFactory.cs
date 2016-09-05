using DragonSpark.Aspects.Validation;
using DragonSpark.Specifications;

namespace DragonSpark.Sources.Parameterized
{
	class AutoValidatingSource<TParameter, TResult> : AutoValidatingSourceBase<TParameter, TResult>, IParameterizedSource<TParameter, TResult>, ISpecification<TParameter>
	{
		/*readonly ISpecification<TParameter> specification;
		readonly IParameterizedSource<TParameter, TResult> inner;*/

		// public AutoValidatingSource( IParameterizedSource<TParameter, TResult> inner ) : this( inner, inner ) {}

		public AutoValidatingSource( ISpecification<TParameter> specification, IParameterizedSource<TParameter, TResult> inner ) : 
			base( new AutoValidationController( new SourceAdapter<TParameter, TResult>( specification ) ), specification.IsSatisfiedBy, inner.Get )
		{
			// this.specification = specification/*.With( Coercer<TParameter>.Default )*/;
			// this.inner = inner/*.With( Coercer<TParameter>.Default )*/;
		}

		// public object Get( object parameter ) => IsSatisfiedBy( parameter ) ? inner.Get( (TParameter)parameter ) : default(TResult);
		
		// public bool IsSatisfiedBy( TParameter parameter ) => specification.IsSatisfiedBy( parameter );
	}
}