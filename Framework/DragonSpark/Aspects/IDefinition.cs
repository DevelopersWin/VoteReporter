using System;
using System.Collections.Generic;
using DragonSpark.Aspects.Build;

namespace DragonSpark.Aspects
{
	public interface IDefinition : IEnumerable<IMethodStore>
	{
		Type DeclaringType { get; }
	}
}