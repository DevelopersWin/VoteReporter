using DragonSpark.Activation;
using DragonSpark.Runtime.Stores;
using DragonSpark.Windows.Runtime;
using System;
using System.Collections.Concurrent;
using System.Reflection;
using System.Threading.Tasks;

namespace DragonSpark.Testing.Framework.Setup
{
	public interface ITaskExecutionContext : IWritableStore<MethodBase>, IDisposable
	{
		TaskContext Id { get; }
	}

	public class ExecutionContextStore : TaskLocalStore<ITaskExecutionContext>, IExecutionContext
	{
		public static ExecutionContextStore Instance { get; } = new ExecutionContextStore();

		readonly ConcurrentDictionary<TaskContext, ITaskExecutionContext> contexts = new ConcurrentDictionary<TaskContext, ITaskExecutionContext>();

		ExecutionContextStore() {}

		protected override ITaskExecutionContext Get() => base.Get() ?? Create();

		ITaskExecutionContext Create()
		{
			var key = TaskContext.Current();
			var result = contexts.GetOrAdd( key, context => new TaskExecutionContext( context, OnRemove ) );
			Assign( result );
			return result;
		}

		void OnRemove( TaskExecutionContext context )
		{
			ITaskExecutionContext removed;
			contexts.TryRemove( context.Id, out removed );
		}

		internal class TaskExecutionContext : FixedStore<MethodBase>, ITaskExecutionContext
		{
			readonly Action<TaskExecutionContext> onDispose;
			
			public TaskExecutionContext( TaskContext id, Action<TaskExecutionContext> onDispose )
			{
				Id = id;
				this.onDispose = onDispose;
			}

			public TaskContext Id { get; }

			public override string ToString() => $"{Id} ({Value})";

			protected override void OnDispose()
			{
				Assign( null );
				onDispose( this );
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

		public override bool Equals( object obj ) => !ReferenceEquals( null, obj ) && ( obj is TaskContext && Equals( (TaskContext)obj ) );

		public override int GetHashCode()
		{
			unchecked
			{
				return ( taskId.GetHashCode() * 397 ) ^ threadId;
			}
		}

		public static bool operator ==( TaskContext left, TaskContext right ) => left.Equals( right );

		public static bool operator !=( TaskContext left, TaskContext right ) => !left.Equals( right );
	}
}