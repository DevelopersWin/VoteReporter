namespace DragonSpark.Testing.Framework.Setup
{
	/*public interface ITaskExecutionContext : IWritableStore<MethodBase>, IDisposable
	{
		Identifier Id { get; }
	}

	public class ExecutionContext : TaskLocalStore<ITaskExecutionContext>, IExecutionContext
	{
		public static ExecutionContext Default { get; } = new ExecutionContext();

		readonly ConcurrentDictionary<Identifier, ITaskExecutionContext> contexts = new ConcurrentDictionary<Identifier, ITaskExecutionContext>();

		ExecutionContext() {}

		protected override ITaskExecutionContext Get() => base.Get() ?? Create();

		ITaskExecutionContext Create()
		{
			var key = Identifier.Current();
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
			
			public TaskExecutionContext( Identifier id, Action<TaskExecutionContext> onDispose )
			{
				Id = id;
				this.onDispose = onDispose;
			}

			public Identifier Id { get; }

			public override string ToString() => $"{Id} ({Value})";

			protected override void OnDispose()
			{
				Assign( null );
				onDispose( this );
			}
		}
	}

	public struct Identifier : IEquatable<Identifier>
	{
		public static Identifier Current() => new Identifier( Environment.CurrentManagedThreadId, Task.CurrentId );

		readonly int threadId;
		readonly int? taskId;

		public Identifier( int threadId, int? taskId = null )
		{
			this.threadId = threadId;
			this.taskId = taskId;
		}

		public override string ToString() => $"Task {taskId} on thread {threadId}";

		public bool Equals( Identifier other ) => taskId == other.taskId && threadId == other.threadId;

		public override bool Equals( object obj ) => !ReferenceEquals( null, obj ) && ( obj is Identifier && Equals( (Identifier)obj ) );

		public override int GetHashCode()
		{
			unchecked
			{
				return ( taskId.GetHashCode() * 397 ) ^ threadId;
			}
		}

		public static bool operator ==( Identifier left, Identifier right ) => left.Equals( right );

		public static bool operator !=( Identifier left, Identifier right ) => !left.Equals( right );
	}*/
}