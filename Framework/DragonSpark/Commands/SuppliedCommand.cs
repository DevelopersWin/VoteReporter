using DragonSpark.Extensions;
using DragonSpark.Sources;
using System;

namespace DragonSpark.Commands
{
	public class DeferredCommand<T> : RunCommandBase
	{
		readonly Func<ICommand<T>> commandSource;
		readonly Func<T> parameterSource;

		public DeferredCommand( ICommand<T> command, Func<T> parameterSource ) : this( command.Self, parameterSource ) {}

		public DeferredCommand( Func<ICommand<T>> commandSource, Func<T> parameterSource )
		{
			this.commandSource = commandSource;
			this.parameterSource = parameterSource;
		}

		public override void Execute() => commandSource().Execute( parameterSource() );

		protected override void OnDispose() => commandSource().TryDispose();
	}

	public class SuppliedCommand<T> : RunCommandBase
	{
		readonly ICommand<T> command;
		readonly T parameter;

		public SuppliedCommand( ICommand<T> command, T parameter )
		{
			this.command = command;
			this.parameter = parameter;
		}

		public override void Execute() => command.Execute( parameter );

		protected override void OnDispose() => command.TryDispose();
	}
}