using System;

namespace DragonSpark.Specifications
{
	public abstract class DelegatedAssignedSpecificationBase<TParameter, TResult> : SpecificationBase<TParameter>
	{
		readonly Func<TParameter, TResult> source;
		readonly Func<TResult, bool> specification;

		protected DelegatedAssignedSpecificationBase( Func<TParameter, TResult> source ) : this( source, AssignedSpecification<TResult>.Default.ToSpecificationDelegate() ) {}

		protected DelegatedAssignedSpecificationBase( Func<TParameter, TResult> source, Func<TResult, bool> specification )
		{
			this.source = source;
			this.specification = specification;
		}

		public override bool IsSatisfiedBy( TParameter parameter ) => specification( source( parameter ) );
	}
}