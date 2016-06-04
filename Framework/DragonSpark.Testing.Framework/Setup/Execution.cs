using DragonSpark.Activation;
using DragonSpark.Extensions;
using DragonSpark.Runtime;
using DragonSpark.Runtime.Stores;
using DragonSpark.Windows.Runtime;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;

namespace DragonSpark.Testing.Framework.Setup
{
	public interface ITaskExecutionContext : IWritableStore<MethodBase>, IDisposable
	{
		TaskContext Id { get; }
	}

	public class ExecutionContext : StoreBase<object>, IExecutionContext
	{
		public static ExecutionContext Instance { get; } = new ExecutionContext();
		protected override object Get()
		{
			var context = ExecutionContextStore.Instance.Value;
			var result = (object)context.Value ?? context;
			return result;
		}
	}

	public class ExecutionContextStore : TaskLocalStore<ITaskExecutionContext>
	{
		public static ExecutionContextStore Instance { get; } = new ExecutionContextStore();

		readonly ConcurrentDictionary<TaskContext, ITaskExecutionContext> contexts = new ConcurrentDictionary<TaskContext, ITaskExecutionContext>();
		readonly Action<TaskExecutionContext> onDispose;
		readonly Action<ITaskExecutionContext> assign;

		ExecutionContextStore()
		{
			onDispose = OnRemove;
			assign = Assign;
		}

		protected override ITaskExecutionContext Get() => base.Get() ?? Create().With( assign );

		ITaskExecutionContext Create()
		{
			var key = TaskContext.Current();
			var result = contexts.GetOrAdd( key, context => new TaskExecutionContext( context, onDispose ).Configured( false ) );
			assign( result );
			return result;
		}

		void OnRemove( TaskExecutionContext context )
		{
			ITaskExecutionContext removed;
			contexts.TryRemove( context.Id, out removed );
		}

		[DebuggerDisplay( "{Id} ({Value})" )]
		internal class TaskExecutionContext : FixedStore<MethodBase>, ITaskExecutionContext
		{
			readonly Action<TaskExecutionContext> onDispose;
			
			public TaskExecutionContext( TaskContext id, Action<TaskExecutionContext> onDispose )
			{
				Id = id;
				this.onDispose = onDispose;
			}

			public TaskContext Id { get; }

			// public override string ToString() => $"{Id} ({Value})";

			protected override void OnDispose()
			{
				Assign( null );
				onDispose( this );
			}
		}
	}

	[DebuggerDisplay( "Task {taskId} on thread {threadId}" )]
	public struct TaskContext : IEquatable<TaskContext>
	{
		public static TaskContext None { get; } = new TaskContext();

		public static TaskContext Current() => new TaskContext( Environment.CurrentManagedThreadId, Task.CurrentId );

		readonly int threadId;
		readonly int? taskId;

		public TaskContext( int threadId, int? taskId = null )
		{
			this.threadId = threadId;
			this.taskId = taskId;
		}

		// public override string ToString() => $;

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