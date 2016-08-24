using System.Collections.Generic;
using System.Threading.Tasks;

namespace DragonSpark.Tasks
{
	public class TaskMonitor : ITaskMonitor
	{
		readonly ICollection<Task> tasks = new List<Task>();

		public void Monitor( Task task ) => tasks.Add( task );

		public void Dispose()
		{
			Task.WhenAll( tasks ).Wait();
			tasks.Clear();
		}
	}
}