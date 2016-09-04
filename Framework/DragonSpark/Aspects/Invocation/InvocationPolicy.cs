using System;

namespace DragonSpark.Aspects.Invocation
{
	interface IInvocationPolicy<T>
	{
		void Execute( T parameter, Action<T> proceed );
	}

	class InvocationPolicy<T> : IInvocationPolicy<T> {
		public void Execute( T parameter, Action<T> proceed ) {}
	}

	/*class CommandSpecificationExecutionPolicy : IExecutionPolicy<object>
	{
		readonly ICommand command;
		public CommandSpecificationExecutionPolicy( ICommand command )
		{
			this.command = command;
		}

		public void Execute( object parameter, Action<object> proceed )
		{
			if ( command.CanExecute( parameter ) )
			{
				proceed( parameter );
			}
		}
	}

	class ExecutionPolicyCollection<T> : CollectionBase<IExecutionPolicy<T>>
	{
		readonly Action<T> body;

		public ExecutionPolicyCollection( Action<T> body )
		{
			this.body = body;
		}
	}*/
}
