using DragonSpark.Commands;
using DragonSpark.Extensions;
using DragonSpark.Sources.Parameterized;
using System.Linq;
using System.Windows.Markup;

namespace DragonSpark.Diagnostics.Configurations
{
	[ContentProperty( nameof(Commands) )]
	public class LoggerConfiguration : AlterationBase<Serilog.LoggerConfiguration>
	{
		public CommandCollection<Serilog.LoggerConfiguration> Commands { get; } = new CommandCollection<Serilog.LoggerConfiguration>();

		public override Serilog.LoggerConfiguration Get( Serilog.LoggerConfiguration parameter ) => Commands.Aggregate( parameter, ( loggerConfiguration, command ) => loggerConfiguration.With( command.Execute ) );
	}
}