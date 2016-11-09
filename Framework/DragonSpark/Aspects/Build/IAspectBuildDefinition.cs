using DragonSpark.Sources.Parameterized;
using PostSharp.Aspects;
using System.Collections.Immutable;
using System.Reflection;

namespace DragonSpark.Aspects.Build
{
	public interface IAspectBuildDefinition : IParameterizedSource<TypeInfo, ImmutableArray<AspectInstance>>, IAspectProvider {}
}