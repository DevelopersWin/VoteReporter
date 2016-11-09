using DragonSpark.Sources.Parameterized;
using DragonSpark.TypeSystem;
using System;
using System.Reflection;

namespace DragonSpark.Aspects.Build
{
	public interface IMethods : ITypeAware, IParameterizedSource<Type, MethodInfo> {}
}