using DragonSpark.Aspects.Build;
using DragonSpark.Aspects.Definitions;
using DragonSpark.Sources.Parameterized;
using System.Collections.Generic;

namespace DragonSpark.Aspects.Validation
{
	sealed class AspectSelection : ParameterizedItemSourceBase<IValidatedTypeDefinition, IAspectDefinition>
	{
		public static AspectSelection Default { get; } = new AspectSelection();
		AspectSelection() {}

		public override IEnumerable<IAspectDefinition> Yield( IValidatedTypeDefinition parameter )
		{
			yield return new MethodAspectDefinition<AutoValidationValidationAspect>( parameter.Validation );
			yield return new MethodAspectDefinition<AutoValidationExecuteAspect>( parameter.Execution );
		}
	}
}