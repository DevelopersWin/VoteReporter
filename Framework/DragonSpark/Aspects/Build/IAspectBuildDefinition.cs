using DragonSpark.Specifications;
using System.Reflection;

namespace DragonSpark.Aspects.Build
{
	public interface IAspectBuildDefinition : ISpecification<TypeInfo>, IAspectProvider<TypeInfo> {}
}