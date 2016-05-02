using DragonSpark.Activation;
using DragonSpark.Extensions;
using PostSharp.Patterns.Contracts;
using System;

namespace DragonSpark.Runtime.Specifications
{
	public class InverseSpecification : SpecificationBase
	{
		readonly ISpecification inner;

		public InverseSpecification( [Required]ISpecification inner )
		{
			this.inner = inner;
		}

		public override bool IsSatisfiedBy( object parameter ) => !inner.IsSatisfiedBy( parameter );
	}

	public class NotNullSpecification : SpecificationBase
	{
		public static ISpecification Instance { get; } = new NotNullSpecification();

		NotNullSpecification() {}
	
		public override bool IsSatisfiedBy( object parameter ) => !parameter.IsNull();
	}

	public abstract class SpecificationBase : ISpecification // <T>
	{
		// bool ISpecification<T>.IsSatisfiedBy( T parameter ) => IsSatisfiedBy( parameter );

		public abstract bool IsSatisfiedBy( object parameter );

	}

	public abstract class GuardedSpecificationBase<T> : ISpecification<T>
	{
		readonly Func<object, T> projection;

		protected GuardedSpecificationBase() : this( Coercer<T>.Instance ) {}

		protected GuardedSpecificationBase( ICoercer<T> coercer ) : this( coercer.Coerce ) {}

		protected GuardedSpecificationBase( Func<object, T> projection )
		{
			this.projection = projection;
		}

		bool ISpecification.IsSatisfiedBy( object parameter ) => projection( parameter ).With( IsSatisfiedBy );

		public abstract bool IsSatisfiedBy( [Required]T parameter );
	}
}