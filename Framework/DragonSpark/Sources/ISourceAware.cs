using System;

namespace DragonSpark.Sources
{
	public interface ISourceAware
	{
		object Get();

		Type SourceType { get; }
	}
}