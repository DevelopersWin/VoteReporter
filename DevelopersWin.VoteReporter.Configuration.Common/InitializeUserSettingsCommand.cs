using DragonSpark.Commands;
using DragonSpark.Specifications;
using DragonSpark.Windows.Setup;
using System.Configuration;

namespace DevelopersWin.VoteReporter.Parts.Common
{
	public sealed class InitializeUserSettingsCommand : SpecificationCommand<ApplicationSettingsBase>
	{
		public static InitializeUserSettingsCommand Default { get; } = new InitializeUserSettingsCommand();
		InitializeUserSettingsCommand() : base( IsDeployedSpecification.Default.Inverse(), DragonSpark.Windows.Setup.InitializeUserSettingsCommand.Default.Execute ) {}
	}
}
