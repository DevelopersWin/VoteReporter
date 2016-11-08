using DragonSpark.Aspects.Build;

namespace DragonSpark.Aspects.Coercion
{
	sealed class Support : AspectBuildDefinition
	{
		public static Support Default { get; } = new Support();
		Support() : base( 
			MethodAspectLocatorFactory<Aspect>.Default,
			CommandTypeDefinition.Default, 
			GeneralizedSpecificationTypeDefinition.Default, 
			GeneralizedParameterizedSourceTypeDefinition.Default ) {}
	}
}