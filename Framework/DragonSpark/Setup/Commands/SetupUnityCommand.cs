using DragonSpark.ComponentModel;
using DragonSpark.Properties;
using PostSharp.Patterns.Contracts;
using Serilog;

namespace DragonSpark.Setup.Commands
{
	public class SetupUnityCommand : ConfigureUnityCommand
	{
		[Locate, Required]
		public ILogger Logger { [return: Required]get; set; }

		public override void Execute( object parameter )
		{
			Logger.Information( Resources.ConfiguringUnityContainer );
			base.Execute( parameter );
		}
	}
}