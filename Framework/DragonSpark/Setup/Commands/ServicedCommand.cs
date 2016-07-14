using DragonSpark.ComponentModel;
using DragonSpark.Runtime;
using DragonSpark.Runtime.Specifications;
using PostSharp.Patterns.Contracts;

namespace DragonSpark.Setup.Commands
{
	// [AutoValidation( false )]
	public abstract class DelegatedFixedCommand<T> : CommandBase<object>
	{
		protected DelegatedFixedCommand() : base( Specifications.Always ) {}

		protected DelegatedFixedCommand( ISpecification<object> specification ) : base( specification ) {}

		public override void Execute( object parameter ) => GetCommand().Execute( GetParameter() );

		public abstract ICommand<T> GetCommand();

		public abstract T GetParameter();
	}

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