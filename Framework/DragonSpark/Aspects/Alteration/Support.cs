using DragonSpark.Aspects.Build;
using PostSharp.Aspects;

namespace DragonSpark.Aspects.Alteration
{
	sealed class Support<T> : AspectBuildDefinition<T> where T : IAspect
	{
		public static Support<T> Default { get; } = new Support<T>();
		Support() : base( GenericCommandCoreTypeDefinition.Default, ParameterizedSourceTypeDefinition.Default ) {}
	}
}