using DragonSpark.Extensions;
using DragonSpark.Specifications;

namespace DragonSpark.Sources.Parameterized
{
	public abstract class ValidatedParameterizedSourceBase<TParameter, TResult> : IValidatedParameterizedSource<TParameter, TResult>
	{
		public static ISpecification<TParameter> DefaultSpecification { get; } = Specifications<TParameter>.Assigned;

		readonly Coerce<TParameter> coercer;
		readonly ISpecification<TParameter> specification;

		protected ValidatedParameterizedSourceBase() : this( DefaultSpecification ) {}
		protected ValidatedParameterizedSourceBase( ISpecification<TParameter> specification ) : this( Defaults<TParameter>.Coercer, specification ) {}

		protected ValidatedParameterizedSourceBase( Coerce<TParameter> coercer, ISpecification<TParameter> specification )
		{
			this.coercer = coercer;
			this.specification = specification;
		}

		public virtual bool IsSatisfiedBy( TParameter parameter ) => specification.IsSatisfiedBy( parameter );
		
		bool ISpecification.IsSatisfiedBy( object parameter ) => specification.IsSatisfiedBy( coercer( parameter ) );

		public abstract TResult Get( TParameter parameter );

		object IParameterizedSource.Get( object parameter ) => GetGeneralized( parameter );

		protected virtual object GetGeneralized( object parameter )
		{
			var coerced = coercer( parameter );
			var result = coerced.IsAssignedOrValue() ? Get( coerced ) : default(TResult);
			return result;
		}
	}
}