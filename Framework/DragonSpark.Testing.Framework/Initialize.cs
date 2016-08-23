using DragonSpark.Extensions;
using DragonSpark.Runtime;
using DragonSpark.Sources;
using DragonSpark.Windows.Runtime;
using PostSharp.Aspects;
using System;
using System.Collections.Concurrent;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DragonSpark.Testing.Framework
{
	public static class Initialize
	{
		[ModuleInitializer( 0 )]
		public static void Execution() => Command.Default.Run();

		sealed class Command : DragonSpark.Setup.Setup
		{
			public static ICommand Default { get; } = new Command();
			Command() : base( 
				Runtime.Application.Execution.Context.Configured( ExecutionContext.Default )
			) {}
		}
	}

	public sealed class Identification : TaskLocalStore<Identifier>
	{
		public static Identification Default { get; } = new Identification();
		Identification() {}

		public override Identifier Get()
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

	public sealed class ExecutionContext : SourceBase<TaskContext>
	{
		public static ISource<TaskContext> Default { get; } = new ExecutionContext( Identification.Default );

		readonly ConcurrentDictionary<Identifier, TaskContext> entries = new ConcurrentDictionary<Identifier, TaskContext>();
		readonly ISource<Identifier> store;
		readonly Func<Identifier, TaskContext> create;
		readonly Action<Identifier> remove;

		ExecutionContext( ISource<Identifier> store )
		{
			this.store = store;
			create = Create;
			remove = Remove;
		}

		public override TaskContext Get() => entries.GetOrAdd( store.Get(), create );

		TaskContext Create( Identifier context ) => new TaskContext( context, remove );

		void Remove( Identifier obj )
		{
			TaskContext removed;
			entries.TryRemove( obj, out removed );
		}
	}

	public sealed class TaskContextFormatter : IFormattable
	{
		readonly TaskContext context;
		public TaskContextFormatter( TaskContext context )
		{
			this.context = context;
		}

		public string ToString( [Optional]string format, [Optional]IFormatProvider formatProvider ) => context.Origin.ToString();
	}

	public sealed class TaskContext : Disposable
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

	public class MethodContext : Scope<MethodBase>
	{
		public static IScope<MethodBase> Default { get; } = new MethodContext();
		MethodContext() {}
	}

	
	/*public class TestingApplicationInitializationCommand : DragonSpark.Setup.Setup
	{
		public TestingApplicationInitializationCommand()
			: base( Windows.InitializationCommand.Default, new DisposeDisposableCommand( ExecutionContext.Default.Value ) )
		{
			Priority = Priority.High;
		}
	}*/

	// class WindowsTestingApplicationInitializationCommand {}
}