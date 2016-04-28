using System;
using DragonSpark.Activation;
using DragonSpark.Extensions;
using DragonSpark.Runtime.Values;
using DragonSpark.TypeSystem;
using PostSharp.Patterns.Contracts;

namespace DragonSpark.Runtime.Specifications
{
	public class InverseSpecification<T> : SpecificationBase<T>
	{
		readonly ISpecification<T> inner;

		public InverseSpecification( [Required]ISpecification<T> inner )
		{
			this.inner = inner;
		}

		protected override bool Verify( T parameter ) => !inner.IsSatisfiedBy( parameter );
	}

	public class NotNullSpecification : NotNullSpecification<object>
	{
		public new static NotNullSpecification Instance { get; } = new NotNullSpecification();
	}
	
	public class NotNullSpecification<T> : SpecificationBase<T>
	{
		public static ISpecification<T> Instance { get; } = new NotNullSpecification<T>();

		readonly Func<T, object> projection;

		// public static ISpecification<T> Null { get; } = Instance.Inverse();

		public NotNullSpecification() : this( arg => arg ) {}

		public NotNullSpecification( Func<T, object> projection )
		{
			this.projection = projection;
		}

		protected override bool Verify( T parameter ) => !projection( parameter ).IsNull();
	}

	public class CheckSpecification<T> : SpecificationBase<T>
	{
		protected override bool Verify( T parameter ) => new Checked( parameter, this ).Value.Apply();
	}

	public abstract class SpecificationBase<T> : ISpecification<T>
	{
		readonly CoercionSupport<T> support;

		protected SpecificationBase() : this( Coercer<T>.Instance ) {}

		protected SpecificationBase( ICoercer<T> coercer ) : this( new CoercionSupport<T>( coercer ) ) {}

		SpecificationBase( CoercionSupport<T> support )
		{
			this.support = support;
		}

		bool ISpecification.IsSatisfiedBy( object parameter ) => support.Coerce( parameter, Verify );

		public bool IsSatisfiedBy( T parameter ) => Verify( parameter );

		protected abstract bool Verify( T parameter );
	}
}