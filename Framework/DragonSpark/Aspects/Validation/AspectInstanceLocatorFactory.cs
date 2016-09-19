using System.Collections.Generic;
using DragonSpark.Aspects.Build;
using DragonSpark.Sources.Parameterized;

namespace DragonSpark.Aspects.Validation
{
	sealed class AspectInstanceLocatorFactory : IParameterizedSource<IValidatedComponentDefinition, IEnumerable<IAspectInstanceLocator>>
	{
		public static AspectInstanceLocatorFactory Default { get; } = new AspectInstanceLocatorFactory();
		AspectInstanceLocatorFactory() {}

		public IEnumerable<IAspectInstanceLocator> Get( IValidatedComponentDefinition parameter )
		{
			yield return new AspectInstanceLocator<AutoValidationValidationAspect>( parameter.Validation );
			yield return new AspectInstanceLocator<AutoValidationExecuteAspect>( parameter.Execution );
		}
	}
}