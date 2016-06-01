using DragonSpark.Activation;
using DragonSpark.Runtime;
using DragonSpark.Runtime.Stores;
using DragonSpark.Windows.Runtime;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace DragonSpark.Testing.Framework.Setup
{
	public interface ITaskExecutionContext : IWritableStore<MethodBase>, IDisposable
	{
		TaskContext Id { get; }

		/*void Attach( TaskContext context );

		void Detach( TaskContext context );

		bool Contains( TaskContext context );*/
	}

	public class ExecutionContext : TaskLocalStore<ITaskExecutionContext>, IExecutionContext
	{
		readonly IDictionary<TaskContext, TaskContext> keys;
		public static ExecutionContext Instance { get; } = new ExecutionContext();

		readonly ConcurrentDictionary<TaskContext, ITaskExecutionContext> contexts = new ConcurrentDictionary<TaskContext, ITaskExecutionContext>();

		ExecutionContext() : this( new ConcurrentDictionary<TaskContext, TaskContext>() ) {}

		ExecutionContext( IDictionary<TaskContext, TaskContext> keys ) : base( new Monitor( keys ).Value )
		{
			this.keys = keys;
		}

		public void Verify( MethodBase method = null)
		{
			var current = TaskContext.Current();
			if ( !keys.ContainsKey( current ) && Value.Id != current )
			{
				throw new InvalidOperationException( $@"'{Value}' does not contain '{current}'" );
			}

			if ( method != null && Value.Value != method )
			{
				throw new InvalidOperationException( $"Assigned Method is different from expected.  Expected: {method}.  Actual: {Value.Value}" );
			}
		}

		protected override ITaskExecutionContext Get() => base.Get() ?? Create();

		ITaskExecutionContext Create()
		{
			var current = TaskContext.Current();
			var key = keys.ContainsKey( current ) ? keys[current] : current;
			if ( keys.ContainsKey( current ) )
			{
				// File.WriteAllText( $@"C:\Temp\Create-{FileSystem.GetValidPath()}.txt", $"{current}" );
				// throw new InvalidOperationException( "Awwww snap!" );
			}
			var result = contexts.GetOrAdd( key, context => new TaskExecutionContext( context, OnRemove ).Configured( false ) );
			// Debug.WriteLine( $"Assigned: {result.Id} ({SynchronizationContext.Current})" );
			Assign( result );
			return result;
		}

		void OnRemove( TaskExecutionContext context )
		{
			ITaskExecutionContext removed;
			contexts.TryRemove( context.Id, out removed );
		}

		class Monitor : StoreBase<AsyncLocal<ITaskExecutionContext>>
		{
			readonly IDictionary<TaskContext, TaskContext> items;
			readonly AsyncLocal<ITaskExecutionContext> store;

			public Monitor( IDictionary<TaskContext, TaskContext> items )
			{
				this.items = items;
				store = new AsyncLocal<ITaskExecutionContext>( OnChange );
			}

			void OnChange( AsyncLocalValueChangedArgs<ITaskExecutionContext> arguments )
			{
				if ( arguments.ThreadContextChanged )
				{
					var current = TaskContext.Current();

					var item = arguments.CurrentValue ?? arguments.PreviousValue;
					if ( item.Id != current )
					{
						if ( arguments.PreviousValue == null )
						{
							if ( items.ContainsKey( current ) )
							{
								// File.WriteAllText( $@"C:\Temp\Add-{FileSystem.GetValidPath()}.txt", $"{current}" );
								throw new InvalidOperationException( $"{this} already contains {current}." );
							}

							items[current] = item.Id;
							//arguments.CurrentValue?.Attach( current );
						}
						else if ( arguments.CurrentValue == null )
						{
							if ( !items.ContainsKey( current ) )
							{
								// File.WriteAllText( $@"C:\Temp\Remove-{FileSystem.GetValidPath()}.txt", $"{current}" );
								throw new InvalidOperationException( $"{this} does not contain {current}." );
							}

							items.Remove( current );
							// arguments.PreviousValue?.Detach( current );
						}
					}
				}
			}

			protected override AsyncLocal<ITaskExecutionContext> Get() => store;
		}

		// [DebuggerDisplay]
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
		public static TaskContext None { get; } = new TaskContext();

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