using DragonSpark.Sources.Parameterized;
using PostSharp.Aspects;
using System.Reflection;

namespace DragonSpark.Aspects.Build
{
	public interface IAspectDefinition : ISpecificationParameterizedSource<TypeInfo, AspectInstance>/*, ITypeAware*/ {}
}