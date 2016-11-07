using JetBrains.Annotations;
using PostSharp.Aspects.Advices;
using System;

namespace DragonSpark.Aspects.Specifications
{
	[IntroduceInterface( typeof(ISpecification) )]
	public sealed class ApplySpecificationAttribute : SpecificationAttributeBase
	{
		public ApplySpecificationAttribute( Type specificationType ) : base( Factory<ApplySpecificationAttribute>.Default.Get( specificationType ) ) {}

		[UsedImplicitly]
		public ApplySpecificationAttribute( ISpecification specification ) : base( specification ) {}
	}
}