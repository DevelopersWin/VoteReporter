using DragonSpark.Activation;
using DragonSpark.Extensions;
using PostSharp.Patterns.Contracts;
using System;

namespace DragonSpark.Runtime.Specifications
{
	public class InverseSpecification : InverseSpecification<object>
	{
		public InverseSpecification( ISpecification inner ) : base( inner ) {}
	}

	public class InverseSpecification<T> : SpecificationBase<T>
	{
		readonly ISpecification inner;

		public InverseSpecification( [Required]ISpecification inner )
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
		readonly Func<object, T> coercer;

		protected SpecificationBase() : this( Coercer<T>.Instance ) {}

		protected SpecificationBase( ICoercer<T> coercer ) : this( coercer.Coerce ) {}

		protected SpecificationBase( Func<object, T> coercer )
		{
			this.coercer = coercer;
		}

		public abstract bool IsSatisfiedBy( T parameter );

		bool ISpecification.IsSatisfiedBy( object parameter ) => IsSatisfiedByCoerced( coercer( parameter ) );

		protected virtual bool IsSatisfiedByCoerced( T parameter ) => IsSatisfiedBy( parameter );
	}

	public abstract class GuardedSpecificationBase<T> : SpecificationBase<T>
	{
		protected GuardedSpecificationBase() {}
		protected GuardedSpecificationBase( ICoercer<T> coercer ) : base( coercer ) {}
		protected GuardedSpecificationBase( Func<object, T> coercer ) : base( coercer ) {}

		protected override bool IsSatisfiedByCoerced( T parameter ) => parameter.With( this.ToDelegate() );
	}
}