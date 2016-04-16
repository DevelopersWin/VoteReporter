namespace DragonSpark.Diagnostics
{
	public class TimerEvents
	{
		public static TimerEvents Instance { get; } = new TimerEvents();

		TimerEvents() : this ( nameof(Starting), nameof(Paused), nameof(Resuming), nameof(Completed) ) {}

		public TimerEvents( string starting, string paused, string resuming, string completed )
		{
			Starting = starting;
			Paused = paused;
			Resuming = resuming;
			Completed = completed;
		}

		public string Starting { get; }
		public string Paused { get; }
		public string Resuming { get; }
		public string Completed { get; }
	}
}