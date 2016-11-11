using DragonSpark.Aspects.Build;
using DragonSpark.Aspects.Definitions;

namespace DragonSpark.Aspects.Coercion
{
	sealed class Definition : AspectBuildDefinition
	{
		public static Definition Default { get; } = new Definition();
		Definition() : base( 
			MethodAspectSelection<Aspect>.Default,

			CommandTypeDefinition.Default, 
			GeneralizedSpecificationTypeDefinition.Default, 
			GeneralizedParameterizedSourceTypeDefinition.Default,
			ParameterizedSourceTypeDefinition.Default,
			GenericCommandCoreTypeDefinition.Default,
			GenericSpecificationTypeDefinition.Default
		) {}
	}
}