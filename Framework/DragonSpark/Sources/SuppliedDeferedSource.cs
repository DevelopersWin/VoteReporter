using System;

namespace DragonSpark.Sources
{
	public class SuppliedDeferedSource<T> : SourceBase<T>
	{
		readonly Lazy<T> lazy;

		public SuppliedDeferedSource( Func<T> factory ) : this( new Lazy<T>( factory ) ) {}

		public SuppliedDeferedSource( Lazy<T> lazy )
		{
			this.lazy = lazy;
		}

		public override T Get() => lazy.Value;
	}
}