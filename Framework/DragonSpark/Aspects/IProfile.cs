using DragonSpark.Aspects.Build;
using System;
using System.Collections.Generic;

namespace DragonSpark.Aspects
{
	public interface IProfile : IEnumerable<IAspectInstance>
	{
		Type DeclaringType { get; }
	}
}