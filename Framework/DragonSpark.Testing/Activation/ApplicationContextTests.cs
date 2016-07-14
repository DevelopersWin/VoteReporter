using DragonSpark.Extensions;
using DragonSpark.Runtime;
using DragonSpark.Runtime.Properties;
using DragonSpark.Runtime.Specifications;
using DragonSpark.Runtime.Stores;
using DragonSpark.Windows.Runtime;
using DragonSpark.Windows.TypeSystem;
using PostSharp.Patterns.Threading;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition.Hosting;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Input;
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

	class ExecutionContextRepository : RepositoryBase<IExecutionContext>
	{
		public static ExecutionContextRepository Instance { get; } = new ExecutionContextRepository();
		ExecutionContextRepository() : base( DefaultExecutionContext.Instance.ToItem() ) {}

		protected override void OnAdd( IExecutionContext entry ) => Store.Ensure( entry );

		protected override void OnInsert( IExecutionContext entry )
		{
			if ( !Store.Contains( entry ) )
			{
				base.OnInsert( entry );
			}
		}

		public object Current() => Query().FirstOrDefault()?.Value;

		public T Get<T>() => Query().Select( context => context.Value ).FirstOrDefaultOfType<T>();

		protected override IEnumerable<IExecutionContext> Query() => Store;
	}

	public interface IExecutionContext : IStore {}

	public class ApplicationContext : CompositeCommand
	{
		public ApplicationContext( params ICommand[] commands ) : base( commands ) {}
	}

	static class Defaults
	{
		public static Func<object> DefaultSource { get; } = ExecutionContextRepository.Instance.Current;
	}

	public abstract class ExecutionContextCachedBase<T> : ExecutionContextStoreBase<T> where T : class
	{
		protected ExecutionContextCachedBase() : base( new Cache<T>() ) {}
		protected ExecutionContextCachedBase( Func<object> contextSource, ICache<object, T> cache ) : base( contextSource, cache ) {}
	}

	public abstract class ExecutionContextStoreBase<T> : WritableStore<T>
	{
		readonly Func<object> contextSource;
		readonly ICache<object, T> cache;

		protected ExecutionContextStoreBase( ICache<object, T> cache ) : this( Defaults.DefaultSource, cache ) {}
		protected ExecutionContextStoreBase( Func<object> contextSource, ICache<object, T> cache )
		{
			this.contextSource = contextSource;
			this.cache = cache;
		}

		protected override T Get() => cache.Get( contextSource() );

		public override void Assign( T item ) => cache.Set( contextSource(), item );
	}

	[Synchronized]
	public class AssignExecutionContextCommand : DelegatedCommand<IExecutionContext>
	{
		public static AssignExecutionContextCommand Instance { get; } = new AssignExecutionContextCommand();
		AssignExecutionContextCommand() : base( ExecutionContextRepository.Instance.Insert ) {}
	}

	class MethodContext : ExecutionContextCachedBase<MethodBase>
	{
		public static MethodContext Instance { get; } = new MethodContext();
		MethodContext() {}
	}

	public interface IInitializationCommand : ICommand, IDisposable {}

	abstract class InitializationCommandBase : CompositeCommand, IInitializationCommand
	{
		protected InitializationCommandBase( params ICommand[] commands ) : base( new OnlyOnceSpecification(), commands ) {}
	}

	class WindowsInitializationCommand : InitializationCommandBase
	{
		public WindowsInitializationCommand() : base( Windows.Configure.Instance ) {}
	}

	class TestingFrameworkInitializationCommand : InitializationCommandBase
	{
		public TestingFrameworkInitializationCommand( MethodBase method ) : base( AssignExecutionContextCommand.Instance.Fixed( ExecutionContext.Instance ), new AssignValueCommand<MethodBase>( MethodContext.Instance ).Fixed( method ) ) {}
	}

	class TestingApplicationInitializationCommand : InitializationCommandBase
	{
		// public TestingApplicationInitializationCommand() : this( typeof(TestingApplicationInitializationCommand).Assembly ) {}
		public TestingApplicationInitializationCommand( MethodBase method ) 
			: base( new TestingFrameworkInitializationCommand( method ), LoadPartsCommand.Instance.Fixed( method.DeclaringType.Assembly ), new WindowsInitializationCommand() ) {}
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