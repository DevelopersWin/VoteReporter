using DragonSpark.TypeSystem;
using System;
using System.Runtime.InteropServices;

namespace DragonSpark.Specifications
{
	public abstract class SpecificationBase<T> : ISpecification<T>
	{
		readonly Coerce<T> coercer;
		readonly Func<T, bool> apply;

		protected SpecificationBase() : this( Sources.Parameterized.Defaults<T>.Coercer ) {}

		protected SpecificationBase( Coerce<T> coercer ) : this( coercer, Where<T>.Assigned ) {}

		protected SpecificationBase( Func<T, bool> apply ) : this( Sources.Parameterized.Defaults<T>.Coercer, apply ) {}

		protected SpecificationBase( Coerce<T> coercer, Func<T, bool> apply )
		{
			this.coercer = coercer;
			this.apply = apply;
		}

		public abstract bool IsSatisfiedBy( T parameter );

		bool ISpecification.IsSatisfiedBy( [Optional]object parameter ) => Coerce( parameter );

		protected virtual bool Coerce( [Optional]object parameter )
		{
			var coerced = coercer( parameter );
			var result = apply( coerced ) && IsSatisfiedBy( coerced );
			return result;
		}

		// protected virtual bool IsSatisfiedByCoerced( T parameter ) => IsSatisfiedBy( parameter );
	}
}