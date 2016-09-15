using System;
using System.Reflection;
using DragonSpark.Sources.Parameterized;

namespace DragonSpark.Aspects
{
	public interface IMethodLocator : IParameterizedSource<Type, MethodInfo>
	{
		Type DeclaringType { get; }
	}
}