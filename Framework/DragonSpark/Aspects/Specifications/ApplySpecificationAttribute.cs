using JetBrains.Annotations;
using System;

namespace DragonSpark.Aspects.Specifications
{
	public sealed class ApplySpecificationAttribute : SpecificationAttributeBase
	{
		public ApplySpecificationAttribute( Type specificationType ) : base( Factory<ApplySpecificationAttribute>.Default.Get( specificationType ) ) {}

		[UsedImplicitly]
		public ApplySpecificationAttribute( ISpecification specification ) : base( specification ) {}
	}
}