using System.Runtime.InteropServices;
using DragonSpark.Sources.Parameterized;

namespace DragonSpark.Specifications
{
	public class CastingSpecification<TFrom, TTo> : SpecificationBase<TTo> where TFrom : TTo
	{
		readonly ISpecification<TFrom> specification;

		public CastingSpecification( ISpecification<TFrom> specification )
		{
			this.specification = specification;
		}

		public override bool IsSatisfiedBy( TTo parameter ) => specification.IsSatisfiedBy( parameter );
	}

	public class DecoratedSpecification<T> : DelegatedSpecification<T>
	{
		readonly ISpecification<T> specification;

		public DecoratedSpecification( ISpecification<T> specification ) : base( specification.ToSpecificationDelegate() )
		{
			this.specification = specification;
		}

		protected override bool Coerce( [Optional]object parameter ) => parameter is T ? IsSatisfiedBy( (T)parameter ) : specification.IsSatisfiedBy( parameter );
	}

	public class OnlyOnceSpecification<T> : ConditionMonitorSpecificationBase<T>
	{
		public OnlyOnceSpecification() : this( new ConditionMonitor() ) {}

		public OnlyOnceSpecification( ConditionMonitor monitor ) : base( monitor.Wrap<T, ConditionMonitor>() ) {}

		
	}
}