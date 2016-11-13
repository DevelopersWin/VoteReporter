using DragonSpark.Aspects.Adapters;
using DragonSpark.Sources;
using DragonSpark.Specifications;
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

	sealed class DefaultSpecificationImplementation<T> : Adapters.SpecificationAdapter<T>, ISpecification<T>
	{
		public DefaultSpecificationImplementation( ISpecification<T> specification ) : base( specification ) {}

		public bool IsSatisfiedBy( T parameter ) => Get( parameter );
	}
}