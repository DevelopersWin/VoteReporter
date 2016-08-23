using System.Runtime.InteropServices;
using DragonSpark.TypeSystem;

namespace DragonSpark.Specifications
{
	public class FixedSpecification : FixedSpecification<object>
	{
		public FixedSpecification( bool satisfied ) : base( satisfied ) {}
	}

	public class FixedSpecification<T> : SpecificationBase<T>
	{
		readonly bool satisfied;

		public FixedSpecification( bool satisfied ) : base( Where<T>.Always )
		{
			this.satisfied = satisfied;
		}

		public override bool IsSatisfiedBy( [Optional]T parameter ) => satisfied;
	}
}