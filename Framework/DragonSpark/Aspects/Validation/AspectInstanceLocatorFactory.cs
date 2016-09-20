using DragonSpark.Aspects.Build;
using DragonSpark.Sources.Parameterized;
using System.Collections.Generic;

namespace DragonSpark.Aspects.Validation
{
	sealed class AspectInstanceLocatorFactory : IParameterizedSource<IValidatedComponentDefinition, IEnumerable<IAspectInstanceLocator>>
	{
		public static AspectInstanceLocatorFactory Default { get; } = new AspectInstanceLocatorFactory();
		AspectInstanceLocatorFactory() {}

		public IEnumerable<IAspectInstanceLocator> Get( IValidatedComponentDefinition parameter )
		{
			yield return new MethodBasedAspectInstanceLocator<AutoValidationValidationAspect>( parameter.Validation );
			yield return new MethodBasedAspectInstanceLocator<AutoValidationExecuteAspect>( parameter.Execution );
		}
	}
}