using DragonSpark.Aspects.Build;
using DragonSpark.Extensions;
using DragonSpark.Specifications;
using DragonSpark.TypeSystem;

namespace DragonSpark.Aspects.Implementations
{
	sealed class Support : AspectBuildDefinition
	{
		public static Support Default { get; } = new Support();
		Support() : base( Descriptors.Default.SelectTypes(), Descriptors.Default.Fixed() ) {}
	}
}