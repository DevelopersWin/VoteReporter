using DragonSpark.Activation.FactoryModel;
using DragonSpark.Aspects;
using DragonSpark.ComponentModel;
using DragonSpark.Composition;
using DragonSpark.Extensions;
using DragonSpark.Runtime;
using DragonSpark.Runtime.Values;
using DragonSpark.TypeSystem;
using PostSharp.Patterns.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Input;
using System.Windows.Markup;

namespace DragonSpark.Setup
{
	public class ApplicationContext : CompositeWritableValue<Assembly[]>
	{
		public ApplicationContext() : this( new AssemblyHost(), new CompositionHost() ) {}

		public ApplicationContext( params IWritableValue<Assembly[]>[] values ) : base( values ) {}
	}

	public class CompositionHost : FixedValue<Assembly[]>
	{
		readonly Func<Assembly[], System.Composition.Hosting.CompositionHost> factory;
		readonly CompositionHostContext context;

		public CompositionHost() : this( CompositionHostFactory.Instance.Create, new CompositionHostContext() ) {}

		public CompositionHost( [Required]Func<Assembly[], System.Composition.Hosting.CompositionHost> factory, [Required]CompositionHostContext context )
		{
			this.factory = factory;
			this.context = context;
		}

		public override void Assign( Assembly[] item )
		{
			var host =  item.With( factory );
			context.Assign( host );

			base.Assign( item );
		}
	}

	public interface IApplication<in T> : ICommand<T>, IApplication
	{
		void Run();
	}

	public interface IApplication : ICommand
	{
		Assembly[] Assemblies { get; }
	}

	public class ApplicationExecutionParameter<T>
	{
		public ApplicationExecutionParameter( [Required]IApplication application, T arguments )
		{
			Application = application;
			Arguments = arguments;
		}

		public IApplication Application { get; }
		public T Arguments { get; }
	}

	public class ApplicationCommandFactory<T> : FactoryBase<ApplicationExecutionParameter<T>, ICommand[]>
	{
		readonly ICommand[] commands;

		public ApplicationCommandFactory( [Required]IEnumerable<ICommand> commands )
		{
			this.commands = commands.Fixed();
		}

		protected override ICommand[] CreateItem( ApplicationExecutionParameter<T> parameter ) => DetermineContextCommands( parameter ).Concat( commands ).ToArray();

		protected virtual IEnumerable<ICommand> DetermineContextCommands( ApplicationExecutionParameter<T> parameter )
		{
			yield return new ProvisionedCommand( new AssignValueCommand<Assembly[]>( new ApplicationContext() ), parameter.Application.Assemblies );
			yield return new ProvisionedCommand( new AmbientContextCommand<ITaskMonitor>(), new TaskMonitor() );
		}
	}

	[ContentProperty( nameof(Body) )]
	public abstract class Application<TParameter> : DisposingCommand<TParameter>, IApplication
	{
		readonly IFactory<ApplicationExecutionParameter<TParameter>, ICommand[]> commandFactory;

		readonly ICollection<IDisposable> disposables = new Collection<IDisposable>();

		protected Application( [Required]Assembly[] assemblies, [Required]IFactory<ApplicationExecutionParameter<TParameter>, ICommand[]> commandFactory )
		{
			Assemblies = assemblies;
			this.commandFactory = commandFactory;
		}

		[Required]
		public Assembly[] Assemblies { [return: Required]get; set; }

		public CommandCollection Body => new CompositeCommand().Commands;

		[Freeze]
		protected override void OnExecute( TParameter parameter )
		{
			disposables.Any().IsTrue( () => { throw new InvalidOperationException( "This application is currently and already executing." ); } );

			var context = new ApplicationExecutionParameter<TParameter>( this, parameter );
			var commands = commandFactory.Create( context ).Concat( Body ).ToArray();
			disposables.AddRange( commands.OfType<IDisposable>() );

			commands.Each( command => command.ExecuteWith<ICommand>( parameter ) );
		}

		protected override void OnDispose() => disposables.Purge().Reverse().Each( disposable => disposable.Dispose() );
	}

	public class ApplyExportedCommandsCommand<T> : Command<object> where T : ICommand
	{
		[Required, Value( typeof(CompositionHostContext) )]
		public System.Composition.Hosting.CompositionHost Host { [return: Required]get; set; }

		public string ContractName { get; set; }

		protected override void OnExecute( object parameter ) => 
			Host.GetExports<T>( ContractName ).Prioritize().Each( setup =>
			{
				setup.ExecuteWith( parameter );
			} );
	}

	public class ApplySetup : ApplyExportedCommandsCommand<ISetup> {}

	public interface ISetup : ICommand<object> {}

	public class Setup : SetupContainerBase<object>, ISetup {}

	public abstract class SetupContainerBase<T> : CompositeCommand<T>
	{
		protected SetupContainerBase( params ICommand<T>[] commands ) : base( commands ) {}

		public Collection<object> Items { get; } = new Collection<object>();
	}

	/*public abstract class Setup : SetupContainerBase<object>, ISetup
	{
		protected override void OnExecute( object parameter )
		{
			using ( new AmbientContextCommand<ISetup>().ExecuteWith( this ) )
			{
				base.OnExecute( parameter );
			}
		}
	}*/
}
