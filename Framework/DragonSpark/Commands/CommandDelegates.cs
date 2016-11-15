using DragonSpark.Sources.Parameterized.Caching;
using System;

namespace DragonSpark.Commands
{
	public sealed class CommandDelegates<T> : Cache<ICommand<T>, Action<T>>
	{
		public static CommandDelegates<T> Default { get; } = new CommandDelegates<T>();
		CommandDelegates() : base( command => command.Execute ) {}
	}
}