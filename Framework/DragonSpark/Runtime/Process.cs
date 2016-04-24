using System;

namespace DragonSpark.Runtime
{
	public interface IProcess : IDisposable
	{
		void Start();
	}

	public interface IContinuation
	{
		void Resume();

		void Pause();
	}
}
