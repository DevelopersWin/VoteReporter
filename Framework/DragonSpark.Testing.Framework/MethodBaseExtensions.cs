namespace DragonSpark.Testing.Framework
{
	public static class MethodBaseExtensions
	{
		/*public static InitializeMethodCommand AsCurrentContext( this MethodBase @this, ILoggerHistory history, LoggingLevelSwitch level ) => AsCurrentContext( @this, new RecordingLoggerFactory( history, level ) );

		public static InitializeMethodCommand AsCurrentContext( this MethodBase @this ) => AsCurrentContext( @this, new RecordingLoggerFactory() );

		public static InitializeMethodCommand AsCurrentContext( this MethodBase @this, RecordingLoggerFactory factory )
		{
			var result = new InitializeMethodCommand().AsExecuted( @this );
			DefaultServiceProvider.Default.Assign( new ServiceProvider( factory ) );
			return result;
		}*/

		// readonly static Func<object, ILogger> LoggerSource = DragonSpark.Diagnostics.Diagnostics.Logger.ToDelegate();

		/*public static IProfiler Profile( this MethodBase method, Action<string> output ) => 
			new ProfilerFactory( output, DragonSpark.Diagnostics.Diagnostics.History.Get( method ), LoggerSource ).Create( method );*/

		/*public static IProfiler Trace( this MethodBase method, Action<string> output )
		{
			var profiler = method.Profile( output );
			var result = profiler.AssociateForDispose( LoggerSource( method ).WithTracing() );
			return result;
		}*/
	}
}