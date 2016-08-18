namespace DragonSpark.Diagnostics.Logging
{
	/*public interface IProfiler : IProcess, IContinuation
	{
		TimeSpan Elapsed { get; }
	}*/

	/*public class Profiler : IProfiler
	{
		readonly ISessionTimer inner;
		readonly EmitProfileEvent handler;
		readonly TimerEvents events;

		public Profiler( ISessionTimer inner, EmitProfileEvent handler ) : this( inner, handler, TimerEvents.Instance ) {}

		public Profiler( ISessionTimer inner, EmitProfileEvent handler, TimerEvents events )
		{
			this.inner = inner;
			this.handler = handler;
			this.events = events;
		}

		public void Start()
		{
			inner.Start();
			handler( events.Starting );
		}

		public void Resume()
		{
			inner.Resume();
			handler( events.Resuming );
		}

		public void Pause()
		{
			inner.Pause();
			handler( events.Paused );
		}

		public void Dispose()
		{
			inner.Dispose();
			handler( events.Completed );
		}

		public TimeSpan Elapsed => inner.Elapsed;
	}*/
}