using DragonSpark.ComponentModel;
using DragonSpark.Modularity;
using DragonSpark.Properties;
using DragonSpark.Runtime;
using PostSharp.Patterns.Contracts;
using Serilog;

namespace DragonSpark.Setup.Commands
{
	public class InitializeModulesCommand : CommandBase<object>
	{
		[Locate, Required]
		public IModuleMonitor Monitor { [return: Required]get; set; }

		[Locate, Required]
		public ILogger MessageLogger { [return: Required]get; set; }

		[Locate, Required]
		public IModuleManager Manager { [return: Required]get; set; }

		[AmbientValue, Required]
		public ITaskMonitor Tasks { [return: Required]get; set; }

		public override void Execute( object parameter )
		{
			MessageLogger.Information( Resources.InitializingModules );
			Manager.Run();

			MessageLogger.Information( Resources.LoadingModules );
			Tasks.Monitor( Monitor.Load().ContinueWith( task =>
			{
				MessageLogger.Information( Resources.ModulesLoaded );
			} ) );
		}
	}
}