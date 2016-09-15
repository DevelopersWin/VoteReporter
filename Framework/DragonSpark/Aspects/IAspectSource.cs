using System;
using DragonSpark.Sources.Parameterized;
using PostSharp.Aspects;

namespace DragonSpark.Aspects
{
	public interface IAspectSource : IParameterizedSource<Type, AspectInstance> {}
}