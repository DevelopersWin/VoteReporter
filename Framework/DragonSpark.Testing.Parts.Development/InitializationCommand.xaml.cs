using DragonSpark.Configuration;
using DragonSpark.Extensions;
using DragonSpark.Setup;
using System.Composition;

namespace DragonSpark.Testing.Parts.Development
{
	[Export( typeof(ISetup) )]
	public partial class InitializationCommand
	{
		public InitializationCommand() : base( EnableMethodCaching.Instance.From( false ) )
		{
			// InitializeComponent();
		}
	}

	/*public class ApplyEnableMethodCachingConfiguration : ApplyParameterizedConfigurationCommand<bool> {}*/
}