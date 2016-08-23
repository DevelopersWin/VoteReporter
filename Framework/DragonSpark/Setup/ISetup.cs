using System;
using DragonSpark.Commands;

namespace DragonSpark.Setup
{
	public interface ISetup : ICommand<object>, IDisposable, IPriorityAware {}
}