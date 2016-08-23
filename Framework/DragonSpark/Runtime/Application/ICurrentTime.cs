using System;

namespace DragonSpark.Runtime.Application
{
	public interface ICurrentTime
	{
		DateTimeOffset Now { get; }
	}
}
