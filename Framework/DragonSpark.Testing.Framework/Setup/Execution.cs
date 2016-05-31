using DragonSpark.Activation;
using DragonSpark.Runtime;
using DragonSpark.Runtime.Stores;
using DragonSpark.Windows.Runtime;
using System;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace DragonSpark.Testing.Framework.Setup
{
	public interface ITaskExecutionContext : IWritableStore<MethodBase>, IDisposable
	{
		TaskContext Id { get; }

		void Add( TaskContext context );

		void Remove( TaskContext context );

		bool Contains( TaskContext context );
	}

	public class ExecutionContext : TaskLocalStore<ITaskExecutionContext>, IExecutionContext
	{
		public static ExecutionContext Instance { get; } = new ExecutionContext();

		readonly ConcurrentDictionary<TaskContext, ITaskExecutionContext> contexts = new ConcurrentDictionary<TaskContext, ITaskExecutionContext>();

		ExecutionContext() : base( new Factory().Value ) {}

		public void Verify()
		{
			var current = TaskContext.Current();
			if ( !Value.Contains( current ) )
			{
				throw new InvalidOperationException( $@"'{Value}' does not contain '{current}'" );
			}
		}

		protected override ITaskExecutionContext Get() => base.Get() ?? Create();

		ITaskExecutionContext Create()
		{
			var result = contexts.GetOrAdd( TaskContext.Current(), context => new TaskExecutionContext( context, OnRemove ).Configured( false ) );
			Assign( result );
			return result;
		}

		void OnRemove( TaskExecutionContext context )
		{
			ITaskExecutionContext removed;
			contexts.TryRemove( context.Id, out removed );
		}

		class Factory : StoreBase<AsyncLocal<ITaskExecutionContext>>
		{
			readonly AsyncLocal<ITaskExecutionContext> store;

			public Factory()
			{
				store = new AsyncLocal<ITaskExecutionContext>( OnChange );
			}

			static void OnChange( AsyncLocalValueChangedArgs<ITaskExecutionContext> arguments )
			{
				if ( arguments.ThreadContextChanged /*&& Task.CurrentId.HasValue*/ )
				{
					if ( arguments.PreviousValue == null ) // new task
					{
						arguments.CurrentValue?.Add( TaskContext.Current() );
					}
					else if ( arguments.CurrentValue == null )
					{
						arguments.PreviousValue?.Remove( TaskContext.Current() );
					}
				}
			}

			protected override AsyncLocal<ITaskExecutionContext> Get() => store;
		}

		// [DebuggerDisplay]
		internal class TaskExecutionContext : FixedStore<MethodBase>, ITaskExecutionContext
		{
			readonly Action<TaskExecutionContext> onDispose;
			readonly ConcurrentDictionary<TaskContext, bool> children = new ConcurrentDictionary<TaskContext, bool>();

			//static void Update( TaskExecutionContext context ) => context.Id = TaskContext.Current();

			// public TaskExecutionContext() : this( TaskContext.Current() ) {}

			public TaskExecutionContext( TaskContext id, Action<TaskExecutionContext> onDispose )
			{
				Id = id;
				this.onDispose = onDispose;
			}

			public TaskContext Id { get; }

			public void Add( TaskContext context )
			{
				if ( Contains( context ) )
				{
					throw new InvalidOperationException( $"{this} already contains {context}." );
				}

				children.TryAdd( context, true );
			}

			public void Remove( TaskContext context )
			{
				bool result;
				if ( Id != context && !children.TryRemove( context, out result ) )
				{
					throw new InvalidOperationException( $@"The provided context '{context}' was not found in root context '{this}'" );
				}
			}

			public bool Contains( TaskContext context ) => Id == context || children.ContainsKey( context );

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