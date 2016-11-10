using DragonSpark.Aspects.Build;
using DragonSpark.Aspects.Definitions;
using DragonSpark.Sources.Parameterized;
using System.Collections.Generic;

namespace DragonSpark.Aspects.Validation
{
	sealed class AspectSelectorFactory : ParameterizedItemSourceBase<IValidatedTypeDefinition, IAspectSelector>
	{
		public static AspectSelectorFactory Default { get; } = new AspectSelectorFactory();
		AspectSelectorFactory() {}

		public override IEnumerable<IAspectSelector> Yield( IValidatedTypeDefinition parameter )
		{
			yield return new MethodAspectSelector<AutoValidationValidationAspect>( parameter.Validation );
			yield return new MethodAspectSelector<AutoValidationExecuteAspect>( parameter.Execution );
		}
	}
}