using DragonSpark.Commands;
using DragonSpark.Extensions;
using DragonSpark.Sources.Parameterized;
using DragonSpark.TypeSystem;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Markup;

namespace DragonSpark.Diagnostics.Configurations
{
	[ContentProperty( nameof(Commands) )]
	public class LoggerConfiguration : AlterationBase<Serilog.LoggerConfiguration>
	{
		public LoggerConfiguration() : this( Items<ICommand<Serilog.LoggerConfiguration>>.Default ) {}

		public LoggerConfiguration( IEnumerable<ICommand<Serilog.LoggerConfiguration>> commands )
		{
			Commands = new CommandCollection<Serilog.LoggerConfiguration>( commands );
		}

		public CommandCollection<Serilog.LoggerConfiguration> Commands { get; }

		public override Serilog.LoggerConfiguration Get( Serilog.LoggerConfiguration parameter ) => Commands.Aggregate( parameter, ( loggerConfiguration, command ) => loggerConfiguration.With( command.Execute ) );
	}
}