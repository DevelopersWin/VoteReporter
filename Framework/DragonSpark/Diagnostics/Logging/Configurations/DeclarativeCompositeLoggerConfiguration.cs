using DragonSpark.Extensions;
using DragonSpark.Runtime;
using DragonSpark.Sources.Parameterized;
using Serilog;
using System.Linq;
using System.Windows.Markup;
using DragonSpark.Commands;

namespace DragonSpark.Diagnostics.Logging.Configurations
{
	[ContentProperty( nameof(Commands) )]
	public class DeclarativeCompositeLoggerConfiguration : AlterationBase<LoggerConfiguration>
	{
		public DeclarativeCollection<CommandBase<LoggerConfiguration>> Commands { get; } = new DeclarativeCollection<CommandBase<LoggerConfiguration>>();

		public override LoggerConfiguration Get( LoggerConfiguration configuration ) => Commands.Aggregate( configuration, ( loggerConfiguration, command ) => loggerConfiguration.With( command.Execute ) );
	}

	/*public class DestructureMethodCommand : DestructureByFactoryCommand<MethodInfo>
	{
		public static DestructureMethodCommand Default { get; } = new DestructureMethodCommand();

		public DestructureMethodCommand() : base( MethodFormatter.Default ) {}
	}*/
}