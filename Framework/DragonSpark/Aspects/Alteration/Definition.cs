using DragonSpark.Aspects.Build;
using DragonSpark.Aspects.Definitions;
using DragonSpark.Sources.Parameterized;
using PostSharp.Aspects;

namespace DragonSpark.Aspects.Alteration
{
	sealed class Definition<T> : AspectBuildDefinition where T : IAspect
	{
		public static Definition<T> Default { get; } = new Definition<T>();
		Definition() : base( 
			MethodAspectLocatorFactory<T>.Default.GetFixed(
				GenericCommandCoreTypeDefinition.Default, 
				ParameterizedSourceTypeDefinition.Default
			)
		) {}
	}
}