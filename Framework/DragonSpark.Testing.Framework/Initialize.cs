using DragonSpark.Activation;
using DragonSpark.Configuration;
using DragonSpark.Runtime;
using DragonSpark.Runtime.Stores;
using DragonSpark.Windows.Runtime;
using PostSharp.Aspects;
using System;
using System.Collections.Concurrent;
using System.Reflection;
using System.Threading.Tasks;

namespace DragonSpark.Testing.Framework
{
	public static class Initialize
	{
		[ModuleInitializer( 0 )] 
		public static void Execute() => ExecutionContextRepository.Instance.Add( ExecutionContextStore.Instance );
	}

	public class TaskContextStore : TaskLocalStore<TaskContext>
	{
		public static TaskContextStore Instance { get; } = new TaskContextStore();
		TaskContextStore() {}

		protected override TaskContext Get()
		{
			var current = base.Get();
			var result = current == default(TaskContext) ? Create() : current;
			return result;
		}

		TaskContext Create()
		{
			var result = TaskContext.Current();
			Assign( result );
			return result;
		}
	}

	public class ExecutionContextStore : StoreBase<ExecutionContext>, IExecutionContextStore
	{
		public static ExecutionContextStore Instance { get; } = new ExecutionContextStore( TaskContextStore.Instance );

		readonly ConcurrentDictionary<TaskContext, ExecutionContext> entries = new ConcurrentDictionary<TaskContext, ExecutionContext>();
		readonly IStore<TaskContext> store;
		readonly Func<TaskContext, ExecutionContext> create;
		readonly Action<TaskContext> remove;

		ExecutionContextStore( IStore<TaskContext> store )
		{
			this.store = store;
			create = Create;
			remove = Remove;
		}

		protected override ExecutionContext Get() => entries.GetOrAdd( store.Value, create );

		ExecutionContext Create( TaskContext context ) => new ExecutionContext( context, remove );

		void Remove( TaskContext obj )
		{
			ExecutionContext removed;
			entries.TryRemove( obj, out removed );
		}
	}

	public class ExecutionContext : Disposable
	{
		readonly Action<TaskContext> complete;

		internal ExecutionContext( TaskContext origin, Action<TaskContext> complete )
		{
			this.complete = complete;
			Origin = origin;
		}

		public TaskContext Origin { get; }

		protected override void OnDispose( bool disposing )
		{
			base.OnDispose( disposing );
			if ( disposing )
			{
				complete( Origin );
			}
		}
	}

	public struct TaskContext : IEquatable<TaskContext>
	{
		public static TaskContext Current() => new TaskContext( Environment.CurrentManagedThreadId, Task.CurrentId );

		readonly int threadId;
		readonly int? taskId;

		public TaskContext( int threadId, int? taskId = null )
		{
			this.threadId = threadId;
			this.taskId = taskId;
		}

		public override string ToString() => $"Task {taskId} on thread {threadId}";

		public bool Equals( TaskContext other ) => taskId == other.taskId && threadId == other.threadId;

		public override bool Equals( object obj ) => !ReferenceEquals( null, obj ) && obj is TaskContext && Equals( (TaskContext)obj );

		public override int GetHashCode()
		{
			unchecked
			{
				return taskId.GetHashCode() * 397 ^ threadId;
			}
		}

		public static bool operator ==( TaskContext left, TaskContext right ) => left.Equals( right );

		public static bool operator !=( TaskContext left, TaskContext right ) => !left.Equals( right );
	}

	public class MethodContext : Configuration<MethodBase>
	{
		public static IConfiguration<MethodBase> Instance { get; } = new MethodContext();
		MethodContext() {}
	}

	
	/*public class TestingApplicationInitializationCommand : DragonSpark.Setup.Setup
	{
		public TestingApplicationInitializationCommand()
			: base( Windows.InitializationCommand.Instance, new DisposeDisposableCommand( ExecutionContextStore.Instance.Value ) )
		{
			Priority = Priority.High;
		}
	}*/

	// class WindowsTestingApplicationInitializationCommand {}
}