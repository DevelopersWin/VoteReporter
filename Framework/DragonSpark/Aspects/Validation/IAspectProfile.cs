using System;
using System.Reflection;
using DragonSpark.Sources.Parameterized;

namespace DragonSpark.Aspects.Validation
{
	public interface IAspectProfile : IParameterizedSource<Type, MethodInfo>
	{
		Type SupportedType { get; }
	}
}