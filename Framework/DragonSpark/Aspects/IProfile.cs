using System;
using System.Collections.Generic;

namespace DragonSpark.Aspects
{
	public interface IProfile : IEnumerable<IAspectSource>
	{
		Type DeclaringType { get; }
	}
}