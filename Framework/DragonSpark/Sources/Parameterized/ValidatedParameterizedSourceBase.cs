using DragonSpark.Specifications;

namespace DragonSpark.Sources.Parameterized
{
	public abstract class ValidatedParameterizedSourceBase<TParameter, TResult> : IValidatedParameterizedSource<TParameter, TResult>
	{
		readonly static ISpecification<TParameter> DefaultSpecification = Specifications<TParameter>.Assigned;

		readonly ISpecification<TParameter> specification;
		readonly ISpecification<object> general;

		protected ValidatedParameterizedSourceBase() : this( DefaultSpecification ) {}

		protected ValidatedParameterizedSourceBase( ISpecification<TParameter> specification )
		{
			this.specification = specification;
			general = specification.Apply( Coercer<TParameter>.Default );
		}

		public virtual bool IsSatisfiedBy( TParameter parameter ) => specification.IsSatisfiedBy( parameter );
		
		bool ISpecification.IsSatisfiedBy( object parameter ) => general.IsSatisfiedBy( parameter );

		public abstract TResult Get( TParameter parameter );

		object IParameterizedSource.Get( object parameter ) => GetGeneralized( parameter );

		protected virtual object GetGeneralized( object parameter ) => Get( (TParameter)parameter );
	}
}