using System;

namespace DragonSpark.Sources
{
	/*public abstract class SourceBase : SourceBase<object>, ISource {}*/

	public abstract class SourceBase<T> : ISource<T>, ISourceAware
	{
		public abstract T Get();

		Type ISourceAware.SourceType => typeof(T);

		object ISourceAware.Get() => Get();
	}
}