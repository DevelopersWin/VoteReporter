using DragonSpark.Activation;
using DragonSpark.Configuration;
using DragonSpark.Extensions;
using DragonSpark.Runtime;
using DragonSpark.Runtime.Stores;
using DragonSpark.Windows.Runtime;
using DragonSpark.Windows.TypeSystem;
using PostSharp.Aspects;
using System;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Composition.Hosting;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace DragonSpark.Testing.Framework
{
	public static class Configure
	{
		[ModuleInitializer( 0 )]
		public static void Initialize() => ExecutionContextRepository.Instance.Add( ExecutionContext.Instance );
	}

	public class ExecutionContextHost : TaskLocalStore<TaskContext>
	{
		public static ExecutionContextHost Instance { get; } = new ExecutionContextHost();
		ExecutionContextHost() {}

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

	public class ExecutionContext : WritableStore<MethodBase>, IExecutionContext
	{
		public static ExecutionContext Instance { get; } = new ExecutionContext( ExecutionContextHost.Instance );

		readonly ConcurrentDictionary<TaskContext, MethodBase> entries = new ConcurrentDictionary<TaskContext, MethodBase>();
		readonly IStore<TaskContext> store;

		ExecutionContext( IStore<TaskContext> store )
		{
			this.store = store;
		}

		protected override MethodBase Get()
		{
			MethodBase method;
			var result = entries.TryGetValue( store.Value, out method ) ? method : null;
			return result;
		}
		
		public override void Assign( MethodBase item )
		{
			var key = store.Value;
			if ( item != null )
			{
				entries.TryAdd( key, item );
			}
			else
			{
				entries.TryRemove( key, out item );
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

	public class TestingFrameworkInitializationCommand : InitializationCommandBase
	{
		public TestingFrameworkInitializationCommand( MethodBase method ) : base( /*AssignExecutionContextCommand.Instance.Fixed( ExecutionContext.Instance ),*/ new AssignValueCommand<MethodBase>( ExecutionContext.Instance ).Fixed( method ) ) {}
	}

	public class LoadPartsCommand : DisposingCommand<Assembly>
	{
		public static LoadPartsCommand Instance { get; } = new LoadPartsCommand();
		LoadPartsCommand() : this( AssemblyPartLocator.Instance.Create ) {}

		readonly Func<Assembly, ImmutableArray<Assembly>> source;

		public LoadPartsCommand( Func<Assembly, ImmutableArray<Assembly>> source )
		{
			this.source = source;
		}

		public override void Execute( Assembly parameter ) => LoadCommand( parameter ).Run();

		CompositeCommand LoadCommand( Assembly parameter )
		{
			var parts = source( parameter ).ToArray();
			var commands = new ContainerConfiguration().WithAssemblies( parts ).CreateContainer().GetExports<IInitializationCommand>().ToArray();
			var result = new CompositeCommand( commands );
			this.AssociateForDispose( result );
			return result;
		}
	}

	public class TestingApplicationInitializationCommand : InitializationCommandBase
	{
		public TestingApplicationInitializationCommand( MethodBase method ) 
			: base( new Windows.InitializationCommand(), new TestingFrameworkInitializationCommand( method ), LoadPartsCommand.Instance.Fixed( method.DeclaringType.Assembly ) ) {}
	}

	class WindowsTestingApplicationInitializationCommand {}
}