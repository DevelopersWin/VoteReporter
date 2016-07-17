using DragonSpark.Configuration;
using System.Composition;

namespace DragonSpark.Testing.Parts.Development
{
	[Export( typeof(IInitializationCommand) )]
	public partial class InitializationCommand
	{
		public InitializationCommand()
		{
			InitializeComponent();
		}
	}

	public class ApplyEnableMethodCachingConfiguration : ApplyValueConfigurationCommand<bool> {}
}