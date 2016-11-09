using DragonSpark.Aspects.Build;
using DragonSpark.Aspects.Definitions;
using DragonSpark.Sources.Parameterized;

namespace DragonSpark.Aspects.Coercion
{
	sealed class Definition : AspectBuildDefinition
	{
		public static Definition Default { get; } = new Definition();
		Definition() : base( 
			MethodAspectLocatorFactory<Aspect>.Default.GetFixed(
				CommandTypeDefinition.Default, 
				GeneralizedSpecificationTypeDefinition.Default, 
				GeneralizedParameterizedSourceTypeDefinition.Default
			)
		) {}
	}
}