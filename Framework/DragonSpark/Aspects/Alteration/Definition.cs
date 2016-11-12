using DragonSpark.Aspects.Build;
using DragonSpark.Aspects.Definitions;
using PostSharp.Aspects;

namespace DragonSpark.Aspects.Alteration
{
	sealed class Definition<T> : AspectBuildDefinition where T : IAspect
	{
		public static Definition<T> Default { get; } = new Definition<T>();
		Definition() : base( 
			MethodAspectSelector<T>.Default, 
			
			CommandTypeDefinition.Default, 
			GeneralizedSpecificationTypeDefinition.Default, 
			GeneralizedParameterizedSourceTypeDefinition.Default,
			ParameterizedSourceTypeDefinition.Default,
			GenericCommandCoreTypeDefinition.Default,
			GenericSpecificationTypeDefinition.Default
		) {}
	}
}