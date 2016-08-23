using DragonSpark.Commands;
using DragonSpark.Sources;

namespace DragonSpark.Application
{
	public class ApplyTaskMonitorCommand : FixedCommand<ITaskMonitor>
	{
		public ApplyTaskMonitorCommand() : base( new AmbientStackCommand<ITaskMonitor>(), new TaskMonitor() ) {}
	}
}