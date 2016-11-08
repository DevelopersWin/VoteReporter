using DragonSpark.Aspects.Build;
using PostSharp.Aspects;

namespace DragonSpark.Aspects.Alteration
{
	sealed class Support<T> : AspectBuildDefinition where T : IAspect
	{
		public static Support<T> Default { get; } = new Support<T>();
		Support() : base( 
			MethodAspectLocatorFactory<T>.Default,
			GenericCommandCoreTypeDefinition.Default, 
			ParameterizedSourceTypeDefinition.Default ) {}
	}
}