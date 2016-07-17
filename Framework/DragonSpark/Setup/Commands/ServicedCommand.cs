using DragonSpark.ComponentModel;
using DragonSpark.Extensions;
using DragonSpark.Runtime;
using DragonSpark.Runtime.Specifications;
using PostSharp.Patterns.Contracts;
using System.Windows.Markup;

namespace DragonSpark.Setup.Commands
{
	public class DeclarativeFixedCommand<T> : DeclarativeCommandBase<T>
	{
		[Required]
		public ICommand<T> Command { [return: Required]get; set; }

		public override void Execute( object parameter ) => Command.Run( Parameter );
	}

	public abstract class DeclarativeCommandBase<T> : DisposingCommand<object>
	{
		protected DeclarativeCommandBase() : base( Specifications.Always ) {}

		[Required]
		public T Parameter { [return: Required]get; set; }
	}

	public abstract class DelegatedFixedCommand<T> : CommandBase<object>
	{
		protected DelegatedFixedCommand() : base( Specifications.Always ) {}

		protected DelegatedFixedCommand( ISpecification<object> specification ) : base( specification ) {}

		public override void Execute( object parameter ) => GetCommand().Execute( GetParameter() );

		public abstract ICommand<T> GetCommand();

		public abstract T GetParameter();
	}

	[ContentProperty( nameof(Parameter) )]
	public class ServicedCommand<TCommand, TParameter> : DelegatedFixedCommand<TParameter> where TCommand : ICommand<TParameter>
	{
		public ServicedCommand() : base( Specifications.Always ) {}

		public ServicedCommand( ISpecification<object> specification ) : base( specification ) {}

		public override ICommand<TParameter> GetCommand() => Command;

		public override TParameter GetParameter() => Parameter;

		[Required, Service]
		public TCommand Command { [return: Required]get; set; }

		[Required, Service]
		public TParameter Parameter { [return: Required]get; set; }
	}
}