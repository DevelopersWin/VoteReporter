using DragonSpark.ComponentModel;
using DragonSpark.Extensions;
using DragonSpark.Runtime;
using DragonSpark.Runtime.Specifications;
using PostSharp.Patterns.Contracts;
using System.Windows.Input;

namespace DragonSpark.Setup.Commands
{
	public abstract class DelegatedFixedCommand : CommandBase<object>
	{
		protected DelegatedFixedCommand() : base( Specifications.Always ) {}

		protected DelegatedFixedCommand( ISpecification<object> specification ) : base( specification ) {}

		protected override void OnExecute( object parameter ) => new FixedCommand( GetCommand, GetParameter ).Run();

		public abstract ICommand GetCommand();

		public abstract object GetParameter();
	}

	public class ServicedCommand<TCommand, TParameter> : DelegatedFixedCommand where TCommand : ICommand<TParameter>
	{
		public ServicedCommand() : base( Specifications.Always ) {}

		public ServicedCommand( ISpecification<object> specification ) : base( specification ) {}

		public override ICommand GetCommand() => Command;

		public override object GetParameter() => Parameter;

		[Required, Service]
		public TCommand Command { [return: Required]get; set; }

		[Required, Service]
		public TParameter Parameter { [return: Required]get; set; }
	}
}