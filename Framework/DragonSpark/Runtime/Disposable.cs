using System;

namespace DragonSpark.Runtime
{
	public class Disposable : IDisposable
	{
		readonly ConditionMonitor monitor = new ConditionMonitor();

		~Disposable()
		{
			Dispose( false );
		}

		public void Dispose()
		{
			Dispose( true );
			GC.SuppressFinalize( this );
		}

		void Dispose( bool disposing )
		{
			if ( monitor.Apply() )
			{
				OnDispose( disposing );
			}
		}

		protected virtual void OnDispose( bool disposing ) {}
	}
}