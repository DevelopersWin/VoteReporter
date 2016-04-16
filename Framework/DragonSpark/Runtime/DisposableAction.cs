using System;

namespace DragonSpark.Runtime
{
	public sealed class DisposableAction : IDisposable
	{
		readonly Action action;

		public DisposableAction( Action action )
		{
			this.action = action;
		}

		public void Dispose() => action();
	}
}