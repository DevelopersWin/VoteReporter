using PostSharp.Patterns.Contracts;
using System;

namespace DragonSpark.Runtime.Specifications
{
	public class WrappedSpecification<T> : SpecificationBase<T>
	{
		readonly ISpecification inner;
		readonly Func<T, object> transform;

		public WrappedSpecification( [Required]ISpecification inner  ) : this( inner, t => t ) {}

		public WrappedSpecification( [Required]ISpecification inner, [Required]Func<T, object> transform )
		{
			this.inner = inner;
			this.transform = transform;
		}

		protected override bool Verify( T parameter ) => inner.IsSatisfiedBy( transform( parameter ) );
	}

	public class OnlyOnceSpecification : SpecificationBase<object>
	{
		readonly ConditionMonitor monitor = new ConditionMonitor();

		protected override bool Verify( object parameter )
		{
			var verify = monitor.Apply();
			return verify;
		}
	}
}