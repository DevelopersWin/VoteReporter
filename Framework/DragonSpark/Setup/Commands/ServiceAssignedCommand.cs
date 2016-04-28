using DragonSpark.ComponentModel;
using DragonSpark.Extensions;
using DragonSpark.Runtime;
using DragonSpark.Runtime.Specifications;
using PostSharp.Patterns.Contracts;
using System.Windows.Input;

namespace DragonSpark.Setup.Commands
{
	public abstract class AssignedCommand : CommandBase<object>
	{
		protected AssignedCommand() : base( AlwaysSpecification.Instance ) {}

		protected AssignedCommand( ISpecification<object> specification ) : base( specification ) {}

		protected override void OnExecute( object parameter ) => new FixedCommand( GetCommand, GetParameter ).Run();
		
		public abstract ICommand GetCommand();

		public abstract object GetParameter();
	}

	public class ServiceAssignedCommand<TCommand, TParameter> : AssignedCommand where TCommand : ICommand<TParameter>
	{
		public ServiceAssignedCommand() : base( AlwaysSpecification.Instance ) {}

		public ServiceAssignedCommand( ISpecification<object> specification ) : base( specification ) {}

		public override ICommand GetCommand() => Command;

		public override object GetParameter() => Parameter;

		[Required, Service]
		public TCommand Command { [return: Required]get; set; }

		[Required, Service]
		public TParameter Parameter { [return: Required]get; set; }
	}
}