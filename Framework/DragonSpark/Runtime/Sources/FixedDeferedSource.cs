using System;

namespace DragonSpark.Runtime.Sources
{
	public class FixedDeferedSource<T> : SourceBase<T>
	{
		readonly Lazy<T> lazy;

		public FixedDeferedSource( Func<T> factory ) : this( new Lazy<T>( factory ) ) {}

		public FixedDeferedSource( Lazy<T> lazy )
		{
			this.lazy = lazy;
		}

		public override T Get() => lazy.Value;
	}
}