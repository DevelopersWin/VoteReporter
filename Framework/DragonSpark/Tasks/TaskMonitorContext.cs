using DragonSpark.Application.Setup;
using DragonSpark.Sources.Parameterized;
using DragonSpark.Sources.Parameterized.Caching;

namespace DragonSpark.Tasks
{
	public sealed class TaskMonitorContext : SuppliedSource<ISetup, ITaskMonitor>
	{
		public static TaskMonitorContext Default { get; } = new TaskMonitorContext();
		TaskMonitorContext() : base( TaskMonitors.Default.Get, AmbientContext<ISetup>.Default.Get ) {}
	}
}