using PostSharp.Patterns.Contracts;
using System.Windows.Input;
using System.Windows.Markup;

namespace DragonSpark.Commands
{
	[ContentProperty( nameof(Parameter) )]
	public class DeclaredFixedCommand : DeclaredCommandBase<object>
	{
		[Required]
		public ICommand Command { [return: Required]get; set; }

		public override void Execute( object parameter ) => Command.Execute( Parameter );
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