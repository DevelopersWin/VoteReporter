using DragonSpark.Activation;
using DragonSpark.Aspects.Adapters;
using DragonSpark.Aspects.Definitions;
using JetBrains.Annotations;
using System;

namespace DragonSpark.Aspects.Implementations
{
	public sealed class IntroduceGeneralizedSpecification : IntroduceInterfaceAspectBase
	{
		public IntroduceGeneralizedSpecification() : this( typeof(DefaultSpecificationImplementation) ) {}

		public IntroduceGeneralizedSpecification( Type implementationType ) : this( ParameterConstructor<object, object>.Make( typeof(ISpecificationAdapter), implementationType ) ) {}

		[UsedImplicitly]
		public IntroduceGeneralizedSpecification( Func<object, object> factory ) : base( GeneralizedSpecificationTypeDefinition.Default, factory ) {}
	}
}