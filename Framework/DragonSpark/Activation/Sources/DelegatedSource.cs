using System;

namespace DragonSpark.Activation.Sources
{
	public class DelegatedSource<T> : SourceBase<T>
	{
		readonly Func<T> get;

		public DelegatedSource( Func<T> get )
		{
			this.get = get;
		}

		public override T Get() => get();
	}
}