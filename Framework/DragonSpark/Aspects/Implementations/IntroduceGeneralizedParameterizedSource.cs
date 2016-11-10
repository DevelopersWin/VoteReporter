using DragonSpark.Activation;
using DragonSpark.Aspects.Adapters;
using DragonSpark.Aspects.Definitions;
using JetBrains.Annotations;
using System;

namespace DragonSpark.Aspects.Implementations
{
	public sealed class IntroduceGeneralizedParameterizedSource : IntroduceInterfaceAspectBase
	{
		public IntroduceGeneralizedParameterizedSource() : this( typeof(DefaultParameterizedSourceImplementation) ) {}

		public IntroduceGeneralizedParameterizedSource( Type implementationType ) : this( ParameterConstructor<object, object>.Make( typeof(IParameterizedSourceAdapter), implementationType ) ) {}

		[UsedImplicitly]
		public IntroduceGeneralizedParameterizedSource( Func<object, object> factory ) : base( GeneralizedParameterizedSourceTypeDefinition.Default.ReferencedType, factory ) {}
	}
}