using DragonSpark.Activation;
using DragonSpark.Runtime.Stores;
using System;

namespace DragonSpark.Runtime
{
	public class AssignExecutionContextCommand<T> : AssignValueCommand<T>
	{
		public AssignExecutionContextCommand() : this( ExecutionStore<T>.Instance ) {}

		public AssignExecutionContextCommand( IWritableStore<T> store ) : base( store ) {}
	}

	public class ExecutionStore<T> : DelegatedWritableStore<T>
	{
		public static ExecutionStore<T> Instance { get; } = new ExecutionStore<T>();

		ExecutionStore() : base( new Func<object>( Execution.GetCurrent ).Convert<T>(), new Action<object>( Execution.Assign ).Convert<T>() ) {}

		// T Get => (T)Execution.GetCurrent();
	}
}
