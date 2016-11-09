using DragonSpark.Aspects.Build;
using DragonSpark.Sources.Parameterized;
using System.Collections.Generic;
using DragonSpark.Aspects.Definitions;

namespace DragonSpark.Aspects.Validation
{
	sealed class AspectInstanceLocatorFactory : IParameterizedSource<IValidatedTypeDefinition, IEnumerable<IAspectSelector>>
	{
		public static AspectInstanceLocatorFactory Default { get; } = new AspectInstanceLocatorFactory();
		AspectInstanceLocatorFactory() {}

		public IEnumerable<IAspectSelector> Get( IValidatedTypeDefinition parameter )
		{
			yield return new MethodAspectSelector<AutoValidationValidationAspect>( parameter.Validation );
			yield return new MethodAspectSelector<AutoValidationExecuteAspect>( parameter.Execution );
		}
	}
}