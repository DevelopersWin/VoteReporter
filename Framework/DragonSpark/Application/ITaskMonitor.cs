using System;
using System.Threading.Tasks;

namespace DragonSpark.Application
{
	public interface ITaskMonitor : IDisposable
	{
		void Monitor( Task task );
	}
}