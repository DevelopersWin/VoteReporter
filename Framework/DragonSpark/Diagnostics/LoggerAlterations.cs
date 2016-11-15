using DragonSpark.Sources;
using DragonSpark.Sources.Parameterized;
using DragonSpark.Sources.Scopes;
using Serilog;

namespace DragonSpark.Diagnostics
{
	public sealed class LoggerAlterations : ItemScope<IAlteration<LoggerConfiguration>>
	{
		public static LoggerAlterations Default { get; } = new LoggerAlterations();
		LoggerAlterations() : base( DefaultLoggerAlterations.Default.IncludeExports ) {}

		/*public sealed class Configure : AssignGlobalScopeCommand<IEnumerable<IAlteration<LoggerConfiguration>>>
		{
			public static Configure Implementation { get; } = new Configure();
			Configure() : base( Default ) {}
		}*/
	}
}