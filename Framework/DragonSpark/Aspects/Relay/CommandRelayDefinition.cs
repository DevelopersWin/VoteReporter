using DragonSpark.Aspects.Build;
using DragonSpark.Aspects.Definitions;
using DragonSpark.Sources.Coercion;
using DragonSpark.Sources.Parameterized;
using System.Collections.Generic;

namespace DragonSpark.Aspects.Relay
{
	public sealed class CommandRelayDefinition : AspectBuildDefinition
	{
		public static CommandRelayDefinition Default { get; } = new CommandRelayDefinition();
		CommandRelayDefinition() : base(
			AspectSelection.Implementation.Accept( ValidatedCastCoercer<ITypeDefinition, IValidatedTypeDefinition>.Default ),
			CommandTypeDefinition.Default
		) {}

		sealed class AspectSelection : ParameterizedItemSourceBase<IValidatedTypeDefinition, IAspects>
		{
			public static AspectSelection Implementation { get; } = new AspectSelection();
			AspectSelection() {}

			public override IEnumerable<IAspects> Yield( IValidatedTypeDefinition parameter )
			{
				yield return new MethodAspects<SpecificationRelay>( parameter.Validation );
				yield return new MethodAspects<CommandRelay>( parameter.Execution );
			}
		}
	}
}