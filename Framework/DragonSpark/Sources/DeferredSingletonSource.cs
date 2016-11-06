using System;

namespace DragonSpark.Sources
{
	public class DeferredSingletonSource<T> : SourceBase<T>
	{
		readonly Lazy<T> lazy;

		public DeferredSingletonSource( Func<T> factory ) : this( new Lazy<T>( factory ) ) {}

		public DeferredSingletonSource( Lazy<T> lazy )
		{
			this.lazy = lazy;
		}

		public override T Get() => lazy.Value;
	}
}