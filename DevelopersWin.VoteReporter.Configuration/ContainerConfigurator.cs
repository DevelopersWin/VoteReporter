using DragonSpark.Composition;
using System.Composition;

namespace DevelopersWin.VoteReporter.Parts
{
	public sealed class ContainerConfigurator : ContainerConfiguratorMappings
	{
		[Export]
		public static ContainerConfigurator Default { get; } = new ContainerConfigurator();
		ContainerConfigurator() : base( Mappings.Default ) {}
	}
}