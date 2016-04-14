using System;

namespace DragonSpark.Diagnostics
{
	public interface IProfiler : IDisposable
	{
		void Start();

		void Event( string name );
	}
}