using DragonSpark.Aspects.Adapters;
using DragonSpark.Sources;
using JetBrains.Annotations;
using PostSharp.Aspects;
using PostSharp.Aspects.Advices;
using System;

namespace DragonSpark.Aspects.Specifications
{
	[IntroduceInterface( typeof(ISource<ISpecificationAdapter>) )]
	public sealed class ApplySpecification : SpecificationAspectBase, ISource<ISpecificationAdapter>, IAspectProvider
	{
		public ApplySpecification( Type specificationType ) : this( specificationType, typeof(SpecificationAdapter<>) ) {}
		public ApplySpecification( Type specificationType, Type implementationType ) : base( Constructors<ApplySpecification>.Default.Get( specificationType ), specificationType, implementationType ) {}

		[UsedImplicitly]
		public ApplySpecification( ISpecificationAdapter specification ) : base( specification ) {}
	}
}