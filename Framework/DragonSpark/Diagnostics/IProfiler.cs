using System;

namespace DragonSpark.Diagnostics
{
	public interface IProfiler : IDisposable
	{
		void Mark( string @event );

		void Start();
	}
}