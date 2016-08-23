using System;
using DragonSpark.Specifications;

namespace DragonSpark.Commands
{
	public class DelegatedFixedCommand<T> : DelegatedFixedCommandBase<T>
	{
		readonly Func<ICommand<T>> command;
		readonly Func<T> parameter;

		public DelegatedFixedCommand( Func<ICommand<T>> command, Func<T> parameter ) : this( command, parameter, Specifications.Specifications.Always ) {}
		public DelegatedFixedCommand( Func<ICommand<T>> command, Func<T> parameter, ISpecification<object> specification ) : base( specification )
		{
			this.command = command;
			this.parameter = parameter;
		}

		public override ICommand<T> GetCommand() => command();

		public override T GetParameter() => parameter();
	}
}