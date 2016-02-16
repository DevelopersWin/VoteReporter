namespace DragonSpark.Diagnostics
{
	public abstract class MessageLoggerBase : IMessageLogger
	{
		public void Log( Message message ) => OnLog( message );

		protected abstract void OnLog( Message message );
	}
}