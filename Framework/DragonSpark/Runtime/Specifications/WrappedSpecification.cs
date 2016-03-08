using PostSharp.Patterns.Contracts;

namespace DragonSpark.Runtime.Specifications
{
	public class WrappedSpecification<T> : SpecificationBase<T>
	{
		readonly ISpecification inner;

		public WrappedSpecification( [Required]ISpecification inner )
		{
			this.inner = inner;
		}

		protected override bool Verify( T parameter ) => inner.IsSatisfiedBy( parameter );
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