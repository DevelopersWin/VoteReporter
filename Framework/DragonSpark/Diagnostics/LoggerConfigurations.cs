using DragonSpark.Diagnostics.Configurations;
using DragonSpark.Runtime.Assignments;
using DragonSpark.Sources;
using DragonSpark.Sources.Scopes;

namespace DragonSpark.Diagnostics
{
	public sealed class LoggerConfigurations : ItemScope<ILoggingConfiguration>
	{
		public static LoggerConfigurations Default { get; } = new LoggerConfigurations();
		LoggerConfigurations() : base( DefaultLoggerConfigurations.Default.IncludeExports ) {}

		public sealed class Configure : AssignGlobalItemScopeCommand<ILoggingConfiguration>
		{
			public static Configure Instance { get; } = new Configure();
			Configure() : base( Default ) {}
		}
	}
}