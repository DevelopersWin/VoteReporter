using DragonSpark.Aspects.Build;

namespace DragonSpark.Aspects.Exceptions
{
	sealed class Support : AspectBuildDefinition
	{
		public static Support Default { get; } = new Support();
		Support() : base( 
			MethodAspectLocatorFactory<Aspect>.Default,
			GenericCommandTypeDefinition.Default, 
			ParameterizedSourceTypeDefinition.Default, 
			GenericSpecificationTypeDefinition.Default ) {}
	}
}