namespace DragonSpark.Sources.Parameterized
{
	/*class AutoValidatingSource<TParameter, TResult> : AutoValidatingSourceBase<TParameter, TResult>, IValidatedParameterizedSource<TParameter, TResult>
	{
		readonly ISpecification<TParameter> specification;
		readonly IParameterizedSource<TParameter, TResult> inner;

		public AutoValidatingSource( IValidatedParameterizedSource<TParameter, TResult> inner ) : this( inner, inner ) {}

		public AutoValidatingSource( ISpecification<TParameter> specification, IParameterizedSource<TParameter, TResult> inner ) : 
			base( new AutoValidationController( new FactoryAdapter<TParameter, TResult>( specification ) ), specification.IsSatisfiedBy, inner.Get )
		{
			this.specification = specification/*.With( Coercer<TParameter>.Default )#1#;
			this.inner = inner/*.With( Coercer<TParameter>.Default )#1#;
		}

		public object Get( object parameter ) => IsSatisfiedBy( parameter ) ? inner.Get( (TParameter)parameter ) : default(TResult);
		
		public bool IsSatisfiedBy( object parameter ) => specification.IsSatisfiedBy( parameter );
	}*/
}