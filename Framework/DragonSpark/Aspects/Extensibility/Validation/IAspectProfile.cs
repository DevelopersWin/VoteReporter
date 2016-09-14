using DragonSpark.Sources.Parameterized;
using System;
using System.Reflection;

namespace DragonSpark.Aspects.Extensibility.Validation
{
	public interface IAspectProfile : IParameterizedSource<Type, MethodInfo>
	{
		Aspects.Extensions.MethodDefinition Method { get; }
		Aspects.Extensions.MethodDefinition Validation { get; }
	}
}