using DragonSpark.Runtime;
using DragonSpark.Sources.Parameterized;
using DragonSpark.TypeSystem;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Markup;

namespace DragonSpark.Diagnostics.Configurations
{
	public interface ILoggingConfiguration : IAlteration<Serilog.LoggerConfiguration> {}

	[ContentProperty( nameof(Commands) )]
	public class LoggingConfigurations : DeclarativeCollection<ILoggingConfiguration>, ILoggingConfiguration
	{
		public LoggingConfigurations() : this( Items<ILoggingConfiguration>.Default ) {}

		public LoggingConfigurations( IEnumerable<ILoggingConfiguration> configurations ) : base( configurations ) {}

		public Serilog.LoggerConfiguration Get( Serilog.LoggerConfiguration parameter ) => this.Aggregate( parameter, ( loggerConfiguration, current ) => current.Get( loggerConfiguration ) );
	}
}