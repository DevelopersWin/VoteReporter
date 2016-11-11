using DragonSpark.Aspects.Build;
using DragonSpark.Aspects.Definitions;
using DragonSpark.Sources.Parameterized;
using System.Collections.Generic;

namespace DragonSpark.Aspects.Validation
{
	sealed class AspectSelection : ParameterizedItemSourceBase<IValidatedTypeDefinition, IAspectSelector>
	{
		public static AspectSelection Default { get; } = new AspectSelection();
		AspectSelection() {}

		public override IEnumerable<IAspectSelector> Yield( IValidatedTypeDefinition parameter )
		{
			yield return new MethodAspectSelector<AutoValidationValidationAspect>( parameter.Validation );
			yield return new MethodAspectSelector<AutoValidationExecuteAspect>( parameter.Execution );
		}
	}
}