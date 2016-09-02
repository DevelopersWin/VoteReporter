using DragonSpark.Extensions;
using System;
using System.Runtime.InteropServices;

namespace DragonSpark.Specifications
{
	public class CoercedSpecification<T> : CoercedSpecification<object, T>
	{
		public CoercedSpecification( Func<object, T> coerce, Func<T, bool> specification ) : base( coerce, specification ) {}
	}

	public class CoercedSpecification<TFrom, TTo> : SpecificationBase<TFrom>, ISpecification<TTo>
	{
		readonly Func<TFrom, TTo> coerce;
		readonly Func<TTo, bool> specification;

		public CoercedSpecification( Func<TFrom, TTo> coerce, Func<TTo, bool> specification )
		{
			this.coerce = coerce;
			this.specification = specification;
		}

		public bool IsSatisfiedBy( [Optional]TTo parameter ) => specification( parameter );

		public override bool IsSatisfiedBy( [Optional]TFrom parameter )
		{
			var to = coerce( parameter );
			var result = to.IsAssignedOrValue() && IsSatisfiedBy( to );
			return result;
		}

		// bool ISpecification.IsSatisfiedBy( object parameter ) => IsSatisfiedBy( coerce( parameter ) );
	}
}