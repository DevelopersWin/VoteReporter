using DragonSpark.Aspects.Adapters;
using DragonSpark.Sources;
using JetBrains.Annotations;
using PostSharp.Aspects.Advices;
using System;

namespace DragonSpark.Aspects.Specifications
{
	[IntroduceInterface( typeof(ISource<ISpecificationAdapter>) )]
	public sealed class ApplySpecification : SpecificationAspectBase, ISource<ISpecificationAdapter>
	{
		public ApplySpecification( Type specificationType ) : this( specificationType, typeof(DefaultSpecificationImplementation<>) ) {}
		public ApplySpecification( Type specificationType, Type implementationType ) : base( Constructors<ApplySpecification>.Default.Get( specificationType ), specificationType, implementationType ) {}

		[UsedImplicitly]
		public ApplySpecification( ISpecificationAdapter specification ) : base( specification ) {}
	}
}