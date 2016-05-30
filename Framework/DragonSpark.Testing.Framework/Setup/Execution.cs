using DragonSpark.Activation;
using DragonSpark.Runtime.Stores;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Runtime.Remoting.Messaging;
using System.Threading;
using System.Threading.Tasks;
using Xunit.Sdk;

namespace DragonSpark.Testing.Framework.Setup
{
	public class ExecutionContext : StoreBase<object>, IExecutionContext
	{
		public static ExecutionContext Instance { get; } = new ExecutionContext();

		readonly ConcurrentDictionary<int, TaskExecutionContext> items = new ConcurrentDictionary<int, TaskExecutionContext>();

		public int count;



		public AsyncLocal<TaskExecutionContext> temp;
		public ExecutionContext()
		{
			temp = new AsyncLocal<TaskExecutionContext>( OnChange );
		}

		void OnChange( AsyncLocalValueChangedArgs<TaskExecutionContext> obj )
		{
			var one = TaskScheduler.Current.Id;
			var two = TaskScheduler.Default.Id;
			var thr = Task.CurrentId;
			var fou = SynchronizationContext.Current;
			var fiv = System.Threading.ExecutionContext.IsFlowSuppressed();
		}

		protected override object Get()
		{
			var one = TaskScheduler.Current.Id;
			var two = TaskScheduler.Default.Id;
			var thr = Task.CurrentId;
			var fou = SynchronizationContext.Current;
			var fiv = System.Threading.ExecutionContext.IsFlowSuppressed();
			/*var current = temp.Value;
			if ( current == null )
			{

				temp.Value = count++;
			}*/
			
			
			// var tasked = SynchronizationContext.Current != null || TaskScheduler.Current.Id != TaskScheduler.Default.Id;
			 
			var result = temp.Value = /*tasked && Task.CurrentId.HasValue*/ /*SynchronizationContext.Current is AsyncTestSyncContext &&*/ Task.CurrentId.HasValue ? items.GetOrAdd( Task.CurrentId.Value, i => new TaskExecutionContext( i ) ) : TaskExecutionContext.Default;
			// Assign( result );
			return result;
		}

		[DebuggerDisplay( "TaskExecutionContext: {TaskId}" )]
		public class TaskExecutionContext : IExecutionContext
		{
			public static TaskExecutionContext Default { get; } = new TaskExecutionContext();

			public TaskExecutionContext( int? taskId = null )
			{
				TaskId = taskId;
			}

			int? TaskId { get; }

			object IStore.Value => TaskId;
		}
	}
}