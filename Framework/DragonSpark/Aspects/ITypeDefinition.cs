using DragonSpark.Aspects.Build;
using System;
using System.Collections.Generic;

namespace DragonSpark.Aspects
{
	public interface ITypeAware
	{
		Type DeclaringType { get; }
	}

	public interface ITypeDefinition : ITypeAware, IEnumerable<IMethodStore> {}
}