using System;

namespace DragonSpark.Diagnostics
{
	public interface IProfiler : IDisposable
	{
		// IProfiler New();

		void Mark( string @event );

		void Start();
	}
}