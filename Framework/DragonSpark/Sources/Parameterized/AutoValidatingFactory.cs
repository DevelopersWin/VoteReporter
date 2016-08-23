using DragonSpark.Aspects.Validation;

namespace DragonSpark.Sources.Parameterized
{
	class AutoValidatingSource<TParameter, TResult> : AutoValidatingSourceBase<TParameter, TResult>, IValidatedParameterizedSource<TParameter, TResult>
	{
		readonly IValidatedParameterizedSource inner;

		public AutoValidatingSource( IValidatedParameterizedSource<TParameter, TResult> inner ) : 
			base( new AutoValidationController( new FactoryAdapter<TParameter, TResult>( inner ) ), inner.IsSatisfiedBy, inner.Get )
		{
			this.inner = inner;
		}

		public object Get( object parameter ) => inner.Get( parameter );
		
		public bool IsSatisfiedBy( object parameter ) => inner.IsSatisfiedBy( parameter );
	}
}