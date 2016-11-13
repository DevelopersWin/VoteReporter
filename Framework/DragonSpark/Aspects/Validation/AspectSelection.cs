using DragonSpark.Aspects.Build;
using DragonSpark.Aspects.Definitions;
using DragonSpark.Sources.Parameterized;
using System.Collections.Generic;

namespace DragonSpark.Aspects.Validation
{
	sealed class AspectSelection : ParameterizedItemSourceBase<IValidatedTypeDefinition, IAspects>
	{
		public static AspectSelection Default { get; } = new AspectSelection();
		AspectSelection() {}

		public override IEnumerable<IAspects> Yield( IValidatedTypeDefinition parameter )
		{
			yield return new MethodAspects<AutoValidationValidationAspect>( parameter.Validation );
			yield return new MethodAspects<AutoValidationExecuteAspect>( parameter.Execution );
		}
	}
}