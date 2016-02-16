using DragonSpark.ComponentModel;
using DragonSpark.Diagnostics;
using DragonSpark.Properties;
using PostSharp.Patterns.Contracts;
using Serilog;

namespace DragonSpark.Setup.Commands
{
	public class SetupUnityCommand : ConfigureUnityCommand
	{
		[Locate, Required]
		public ILogger Logger { [return: Required]get; set; }

		protected override void OnExecute( object parameter )
		{
			Logger.Information( Resources.ConfiguringUnityContainer );
			base.OnExecute( parameter );
		}
	}
}