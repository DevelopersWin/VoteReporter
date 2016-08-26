using DragonSpark.Windows.Runtime;

namespace DevelopersWin.VoteReporter.Application
{
	sealed class Commands : DragonSpark.Application.ApplicationCommandSource
	{
		public static Commands Default { get; } = new Commands();
		Commands() : base( FileSystemTypes.Default, DragonSpark.Composition.ServiceProviderConfigurations.Default ) {}
	}
}