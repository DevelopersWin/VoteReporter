using JetBrains.Annotations;
using System;

namespace DragonSpark.Sources
{
	public class DelegatedSingletonSource<T> : SourceBase<T>
	{
		readonly Lazy<T> lazy;

		public DelegatedSingletonSource( Func<T> factory ) : this( new Lazy<T>( factory ) ) {}

		[UsedImplicitly]
		public DelegatedSingletonSource( Lazy<T> lazy )
		{
			this.lazy = lazy;
		}

		public override T Get() => lazy.Value;
	}
}