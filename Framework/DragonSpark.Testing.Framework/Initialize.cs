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
		public static void Execute() => Execution.Context.Assign( ExecutionContext.Instance );
	}

	public class IdentificationStore : TaskLocalStore<Identifier>
	{
		public static IdentificationStore Instance { get; } = new IdentificationStore();
		IdentificationStore() {}

		protected override Identifier Get()
		{
			var current = base.Get();
			var result = current == default(Identifier) ? Create() : current;
			return result;
		}

		Identifier Create()
		{
			var result = Identifier.Current();
			Assign( result );
			return result;
		}
	}

	public class ExecutionContext : StoreBase<TaskContext>
	{
		public static ISource<TaskContext> Instance { get; } = new ExecutionContext( IdentificationStore.Instance );

		readonly ConcurrentDictionary<Identifier, TaskContext> entries = new ConcurrentDictionary<Identifier, TaskContext>();
		readonly IStore<Identifier> store;
		readonly Func<Identifier, TaskContext> create;
		readonly Action<Identifier> remove;

		ExecutionContext( IStore<Identifier> store )
		{
			this.store = store;
			create = Create;
			remove = Remove;
		}

		protected override TaskContext Get() => entries.GetOrAdd( store.Value, create );

		TaskContext Create( Identifier context ) => new TaskContext( context, remove );

		void Remove( Identifier obj )
		{
			TaskContext removed;
			entries.TryRemove( obj, out removed );
		}
	}

	public class TaskContext : Disposable
	{
		readonly Action<Identifier> complete;

		internal TaskContext( Identifier origin, Action<Identifier> complete )
		{
			this.complete = complete;
			Origin = origin;
		}

		public Identifier Origin { get; }

		protected override void OnDispose( bool disposing )
		{
			base.OnDispose( disposing );
			if ( disposing )
			{
				complete( Origin );
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

		public override bool Equals( object obj ) => !ReferenceEquals( null, obj ) && obj is Identifier && Equals( (Identifier)obj );

		public override int GetHashCode()
		{
			unchecked
			{
				return taskId.GetHashCode() * 397 ^ threadId;
			}
		}

		public static bool operator ==( Identifier left, Identifier right ) => left.Equals( right );

		public static bool operator !=( Identifier left, Identifier right ) => !left.Equals( right );
	}

	public class MethodContext : Configuration<MethodBase>
	{
		public static IConfiguration<MethodBase> Instance { get; } = new MethodContext();
		MethodContext() {}
	}

	
	/*public class TestingApplicationInitializationCommand : DragonSpark.Setup.Setup
	{
		public TestingApplicationInitializationCommand()
			: base( Windows.InitializationCommand.Instance, new DisposeDisposableCommand( ExecutionContext.Instance.Value ) )
		{
			Priority = Priority.High;
		}
	}*/

	// class WindowsTestingApplicationInitializationCommand {}
}