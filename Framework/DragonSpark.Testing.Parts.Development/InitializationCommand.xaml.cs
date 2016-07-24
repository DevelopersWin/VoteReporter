using DragonSpark.Configuration;
using System.Composition;

namespace DragonSpark.Testing.Parts.Development
{
	[Export( typeof(IInitializationCommand) )]
	public partial class InitializationCommand
	{
		public InitializationCommand() : base( EnableMethodCaching.Instance.From( false ) )
		{
			// InitializeComponent();
		}
	}

	/*public class ApplyEnableMethodCachingConfiguration : ApplyConfigurationCommand<bool> {}*/
}