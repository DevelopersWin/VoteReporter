using DragonSpark.Commands;
using DragonSpark.Extensions;
using DragonSpark.Specifications;
using PostSharp.Patterns.Contracts;
using System;
using System.Windows.Markup;

namespace DragonSpark.Setup.Commands
{
	[ContentProperty( nameof(Parameter) )]
	public class DeclaredFixedCommand : DeclaredCommandBase<object>
	{
		[Required]
		public ICommand<object> Command { [return: Required]get; set; }

		public override void Execute( object parameter ) => Command.Run( Parameter );
	}

	public abstract class DeclaredCommandBase<T> : CommandBase<object>
	{
		protected DeclaredCommandBase( T parameter = default(T) ) : base( Specifications.Specifications.Always )
		{
			if ( parameter.IsAssigned() )
			{
				Parameter = parameter;
			}
		}

		[Required]
		public T Parameter { [return: Required]get; set; }
	}

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

	public abstract class DelegatedFixedCommandBase<T> : CommandBase<object>
	{
		protected DelegatedFixedCommandBase() : base( Specifications.Specifications.Always ) {}

		protected DelegatedFixedCommandBase( ISpecification<object> specification ) : base( specification ) {}

		public override void Execute( object parameter ) => GetCommand().Execute( GetParameter() );

		public abstract ICommand<T> GetCommand();

		public abstract T GetParameter();
	}

	/*[ContentProperty( nameof(Parameter) )]
	public class ServicedCommand<TCommand, TParameter> : DelegatedFixedCommandBase<TParameter> where TCommand : ICommand<TParameter>
	{
		public ServicedCommand() : base( Specifications.Always ) {}

		public ServicedCommand( ISpecification<object> specification ) : base( specification ) {}

		public override ICommand<TParameter> GetCommand() => Command;

		public override TParameter GetParameter() => Parameter;

		[Required, Service]
		public TCommand Command { [return: Required]get; set; }

		[Required, Service]
		public TParameter Parameter { [return: Required]get; set; }
	}*/
}