using DragonSpark.Runtime;
using DragonSpark.Sources.Parameterized;
using System;
using System.Reflection;

namespace DragonSpark.Aspects.Extensibility.Validation
{
	public interface IAspectProfile : IParameterizedSource<Type, MethodInfo>
	{
		MethodDescriptor Method { get; }
		MethodDescriptor Validation { get; }
	}
}