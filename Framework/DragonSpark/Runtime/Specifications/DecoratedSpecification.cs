using DragonSpark.Activation;
using System;

namespace DragonSpark.Runtime.Specifications
{
	public class DecoratedSpecification<T> : DelegatedSpecification<T>
	{
		public DecoratedSpecification( ISpecification<T> inner ) : this( inner, Coercer<T>.Instance ) {}
		public DecoratedSpecification( ISpecification<T> inner, ICoercer<T> coercer ) : base( inner.IsSatisfiedBy, coercer ) {}
	}

	public class BoxedSpecification<T> : SpecificationBase<T>
	{
		readonly ISpecification specification;
		readonly Func<T, object> box;

		public BoxedSpecification( ISpecification specification ) : this( specification, t => t ) {}

		public BoxedSpecification( ISpecification specification, Func<T, object> box )
		{
			this.specification = specification;
			this.box = box;
		}

		protected override bool Verify( T parameter ) => specification.IsSatisfiedBy( box( parameter ) );
	}

	public class DelegatedSpecification<T> : SpecificationBase<T>
	{
		readonly Func<T, bool> @delegate;

		public DelegatedSpecification( Func<T, bool> @delegate ) : this( @delegate, Coercer<T>.Instance ) {}

		public DelegatedSpecification( Func<T, bool> @delegate, ICoercer<T> coercer ) : base( coercer )
		{
			this.@delegate = @delegate;
		}

		protected override bool Verify( T parameter ) => @delegate( parameter );
	}

	public class OnlyOnceSpecification : SpecificationBase<object>
	{
		readonly ConditionMonitor monitor = new ConditionMonitor();

		protected override bool Verify( object parameter ) => monitor.Apply();
	}
}