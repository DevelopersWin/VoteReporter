using DragonSpark.Aspects.Adapters;
using JetBrains.Annotations;
using System;

namespace DragonSpark.Aspects.Specifications
{
	public sealed class ApplySpecificationAttribute : SpecificationAspectBase
	{
		public ApplySpecificationAttribute( Type specificationType ) : base( Factory<ApplySpecificationAttribute>.Default.Get( specificationType ) ) {}

		[UsedImplicitly]
		public ApplySpecificationAttribute( ISpecificationAdapter specification ) : base( specification ) {}
	}
}