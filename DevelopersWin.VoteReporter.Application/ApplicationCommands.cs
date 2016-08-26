using DragonSpark.Windows.Runtime;

namespace DevelopersWin.VoteReporter.Application
{
	sealed class ApplicationCommands : DragonSpark.Application.ApplicationCommandSource
	{
		public static ApplicationCommands Default { get; } = new ApplicationCommands();
		ApplicationCommands() : base( FileSystemTypes.Default, DragonSpark.Composition.ServiceProviderConfigurations.Default ) {}
	}
}