using DragonSpark.ComponentModel;
using DragonSpark.Extensions;
using DragonSpark.Runtime;
using PostSharp.Patterns.Contracts;

namespace DragonSpark.Setup.Commands
{
	public class ServicedCommand<TCommand, TParameter> : SetupCommandBase where TCommand : ICommand<TParameter>
	{
		protected override void OnExecute( object parameter ) => Command.ExecuteWith( Parameter );

		[Required, Service]
		public TCommand Command { [return: Required]get; set; }

		[Required, Service]
		public TParameter Parameter { [return: Required]get; set; }
	}
}