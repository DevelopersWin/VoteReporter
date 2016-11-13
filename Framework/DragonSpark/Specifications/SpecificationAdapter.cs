using System;

namespace DragonSpark.Specifications
{
	public sealed class SpecificationAdapter<T> : SpecificationBase<T>
	{
		readonly Func<bool> factory;

		public SpecificationAdapter( Func<bool> factory )
		{
			this.factory = factory;
		}

		public override bool IsSatisfiedBy( T parameter ) => factory();
	}
}