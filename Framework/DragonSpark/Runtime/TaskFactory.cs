using DragonSpark.Activation;
using DragonSpark.Extensions;
using DragonSpark.Runtime.Values;
using System;
using System.Threading.Tasks;

namespace DragonSpark.Runtime
{
	public class AssignExecutionContextCommand : AssignValueCommand<object>
	{
		public AssignExecutionContextCommand() : this( ExecutionContextStore.Instance ) {}

		public AssignExecutionContextCommand( IWritableStore<object> store ) : base( store ) {}
	}

	public class ExecutionContextStore : WritableStore<object>
	{
		public static ExecutionContextStore Instance { get; } = new ExecutionContextStore();

		ExecutionContextStore() {}

		protected override object Get() => Execution.Current;

		public override void Assign( object item ) => Execution.Assign( item );
	}

	public class TaskFactory : FactoryBase<Action, Task>
	{
		public static TaskFactory Instance { get; } = new TaskFactory();

		protected override Task CreateItem( Action parameter )
		{
			var current = Execution.Current;
			var result = Task.Factory.StartNew( () =>
												{
													using ( new AssignExecutionContextCommand().AsExecuted( current ) )
													{
														parameter();
													}
												} );
			return result;
		}
	}
}
