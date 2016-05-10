using System;

namespace DragonSpark.Runtime.Specifications
{
	public abstract class CompositeSpecification<T> : SpecificationBase<T>
	{
		readonly Func<Func<ISpecification, bool>, bool> where;
		
		protected CompositeSpecification( Func<Func<ISpecification, bool>, bool> where )
		{
			this.where = where;
		}

		public override bool IsSatisfiedBy( T parameter ) => where( condition => condition.IsSatisfiedBy( parameter ) );
	}
}