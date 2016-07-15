using DragonSpark.Activation;
using DragonSpark.Configuration;
using DragonSpark.Extensions;
using DragonSpark.Runtime;
using DragonSpark.Runtime.Properties;
using DragonSpark.Runtime.Stores;
using DragonSpark.Windows.Runtime;
using DragonSpark.Windows.TypeSystem;
using System;
using System.Collections.Immutable;
using System.Composition.Hosting;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Xunit;

namespace DragonSpark.Testing.Activation
{
	public class ApplicationContextTests
	{
		[Fact]
		public void Assignment()
		{
			/*using ( var context = ApplicationContextCache.Instance.Get( MethodBase.GetCurrentMethod() ) )
			{
				AssignExecutionContextCommand.Instance.Execute( ExecutionContext.Instance );

				// new AssignValueCommand<MethodInfo>()
			}*/
		}
	}

	class DefaultExecutionContext : Store<AppDomain>, IExecutionContext
	{
		public static DefaultExecutionContext Instance { get; } = new DefaultExecutionContext();
		DefaultExecutionContext() : base( AppDomain.CurrentDomain ) {}
	}

	class ExecutionContext : TaskLocalStore<ExecutionContext.TaskContext>, IExecutionContext
	{
		public static ExecutionContext Instance { get; } = new ExecutionContext();
		ExecutionContext() {}

		readonly EqualityReference<TaskContext> references = EqualityReference<TaskContext>.Instance;

		protected override TaskContext Get() => base.Get() ?? Create();

		TaskContext Create()
		{
			var result = references.Create( TaskContext.Current() );
			Assign( result );
			return result;
		}

		public class TaskContext : IEquatable<TaskContext>
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
					return ( taskId.GetHashCode() * 397 ) ^ threadId;
				}
			}

			public static bool operator ==( TaskContext left, TaskContext right ) => left.Equals( right );

			public static bool operator !=( TaskContext left, TaskContext right ) => !left.Equals( right );
		}
	}

	/*class WindowsInitializationCommand : InitializationCommandBase
	{
		public WindowsInitializationCommand() : base( Windows.Configure.Instance ) {}
	}*/

	class TestingFrameworkInitializationCommand : InitializationCommandBase
	{
		public TestingFrameworkInitializationCommand( MethodBase method ) : base( AssignExecutionContextCommand.Instance.Fixed( ExecutionContext.Instance ), new AssignValueCommand<MethodBase>( MethodContext.Instance ).Fixed( method ) ) {}
	}

	class TestingApplicationInitializationCommand : InitializationCommandBase
	{
		public TestingApplicationInitializationCommand( MethodBase method ) 
			: base( new TestingFrameworkInitializationCommand( method ), LoadPartsCommand.Instance.Fixed( method.DeclaringType.Assembly ), new Windows.Configure() ) {}
	}

	class WindowsTestingApplicationInitializationCommand {}

	class LoadPartsCommand : DisposingCommand<Assembly>
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

	/*class CurrentApplicationContext : ExecutionContextStoreBase<WindowsTestingApplicationContext>
	{
		public static CurrentApplicationContext Instance { get; } = new CurrentApplicationContext();
		CurrentApplicationContext() : base( new Cache<WindowsTestingApplicationContext>() ) {}
	}*/

	/*class ApplicationContextCache : ParameterConstructedCache<MethodBase, WindowsTestingApplicationContext>
	{
		public static ApplicationContextCache Instance { get; } = new ApplicationContextCache();
		ApplicationContextCache() {}
	}*/



	/*class WindowsTestingApplicationContext : ApplicationContext
	{
		public WindowsTestingApplicationContext( MethodBase method ) : base( Windows.Configure.Instance/*, LoadPartsCommand.Instance#1# )
		{
			Method = method;
		}

		public MethodBase Method { get; }

		/*class LoadPartsCommand : FixedCommand
		{
			public static LoadPartsCommand Instance { get; } = new LoadPartsCommand();

			LoadPartsCommand() : base( LoadPartAssemblyCommand.Instance, typeof(LoadPartsCommand).Assembly ) {}
		}#1#
	}*/
	
}