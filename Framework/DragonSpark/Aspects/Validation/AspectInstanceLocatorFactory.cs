using DragonSpark.Aspects.Build;
using DragonSpark.Aspects.Definitions;
using DragonSpark.Sources.Parameterized;
using System.Collections.Generic;

namespace DragonSpark.Aspects.Validation
{
	sealed class AspectInstanceLocatorFactory : IParameterizedSource<IValidatedTypeDefinition, IEnumerable<IAspectSource>>
	{
		public static AspectInstanceLocatorFactory Default { get; } = new AspectInstanceLocatorFactory();
		AspectInstanceLocatorFactory() {}

		public IEnumerable<IAspectSource> Get( IValidatedTypeDefinition parameter )
		{
			yield return new MethodAspectSource<AutoValidationValidationAspect>( parameter.Validation );
			yield return new MethodAspectSource<AutoValidationExecuteAspect>( parameter.Execution );
		}
	}
}