using DragonSpark.Sources.Parameterized;
using PostSharp.Aspects;
using System.Reflection;

namespace DragonSpark.Aspects.Build
{
	public interface IAspects : ISpecificationParameterizedSource<TypeInfo, AspectInstance>/*, ITypeAware*/ {}
}