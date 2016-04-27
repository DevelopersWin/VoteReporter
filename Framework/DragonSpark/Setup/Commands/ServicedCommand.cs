using DragonSpark.ComponentModel;
using DragonSpark.Extensions;
using DragonSpark.Runtime;
using DragonSpark.Runtime.Specifications;
using PostSharp.Patterns.Contracts;

namespace DragonSpark.Setup.Commands
{
	public class ServicedCommand<TCommand, TParameter> : CommandBase<object> where TCommand : ICommand<TParameter>
	{
		public ServicedCommand() : base( AlwaysSpecification.Instance ) {}

		public ServicedCommand( ISpecification<object> specification ) : base( specification ) {}

		protected override void OnExecute( object parameter ) => Command.Executed( Parameter );

		[Required, Service]
		public TCommand Command { [return: Required]get; set; }

		[Required, Service]
		public virtual TParameter Parameter { [return: Required]get; set; }
	}
}