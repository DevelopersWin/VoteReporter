using DragonSpark.Activation;
using DragonSpark.Extensions;
using System;
using DragonSpark.Sources.Parameterized;

namespace DragonSpark.Runtime.Specifications
{
	public class InverseSpecification : InverseSpecification<object>
	{
		public InverseSpecification( ISpecification inner ) : base( inner ) {}
	}

	public class InverseSpecification<T> : SpecificationBase<T>
	{
		readonly ISpecification inner;

		public InverseSpecification( ISpecification inner )
		{
			this.inner = inner;
		}

		public override bool IsSatisfiedBy( T parameter ) => !inner.IsSatisfiedBy( parameter );
	}

	public class AssignedSpecification<T> : SpecificationBase<T>
	{
		public static ISpecification<T> Instance { get; } = new AssignedSpecification<T>();

		AssignedSpecification() {}
	
		public override bool IsSatisfiedBy( T parameter ) => parameter.IsAssigned();
	}

	public abstract class SpecificationBase<T> : ISpecification<T>
	{
		readonly Coerce<T> coercer;

		protected SpecificationBase() : this( Defaults<T>.Coercer ) {}

		protected SpecificationBase( Coerce<T> coercer )
		{
			this.coercer = coercer;
		}

		public abstract bool IsSatisfiedBy( T parameter );

		bool ISpecification.IsSatisfiedBy( object parameter ) => IsSatisfiedByCoerced( coercer( parameter ) );

		protected virtual bool IsSatisfiedByCoerced( T parameter ) => IsSatisfiedBy( parameter );
	}

	public abstract class GuardedSpecificationBase<T> : SpecificationBase<T>
	{
		readonly Func<T, bool> isSatisfiedBy;

		protected GuardedSpecificationBase() : this( Defaults<T>.Coercer ) {}
		protected GuardedSpecificationBase( Coerce<T> coercer ) : base( coercer )
		{
			isSatisfiedBy = IsSatisfiedBy;
		}

		protected override bool IsSatisfiedByCoerced( T parameter ) => parameter.With( isSatisfiedBy );
	}
}