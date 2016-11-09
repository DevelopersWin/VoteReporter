using JetBrains.Annotations;
using PostSharp.Aspects.Advices;
using System;
using DragonSpark.Aspects.Adapters;

namespace DragonSpark.Aspects.Specifications
{
	[IntroduceInterface( typeof(ISpecification) )]
	public sealed class ApplyInverseSpecificationAttribute : SpecificationAttributeBase
	{
		public ApplyInverseSpecificationAttribute( Type specificationType ) : base( Factory<ApplyInverseSpecificationAttribute>.Default.Get( specificationType ) ) {}

		[UsedImplicitly]
		public ApplyInverseSpecificationAttribute( ISpecification specification ) : base( new Specification( specification ) ) {}

		sealed class Specification : InvocationBase<object, bool>, ISpecification
		{
			readonly ISpecification specification;

			public Specification( ISpecification specification )
			{
				this.specification = specification;
			}

			public override bool Invoke( object parameter ) => !(bool)specification.Get( parameter );
		}
	}
}