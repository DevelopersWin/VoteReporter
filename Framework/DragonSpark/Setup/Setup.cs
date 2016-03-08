using DragonSpark.Activation.FactoryModel;
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
using DragonSpark.Runtime.Specifications;

namespace DragonSpark.Setup
{
	public class AssignApplication : AssignValueCommand<Assembly[]>
	{
		public AssignApplication() : this( new ApplicationContext() ) {}

		public AssignApplication( IWritableValue<Assembly[]> value ) : base( value ) {}
	}

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

	/*public interface IApplication<in T> : ICommand<T>, IApplication
	{
		void Run();
	}*/

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
			yield return new ProvisionedCommand( new AssignApplication(), parameter.Application.Assemblies );
			yield return new ProvisionedCommand( new AmbientContextCommand<ITaskMonitor>(), new TaskMonitor() );
		}
	}

	public abstract class Application<TParameter> : CompositeCommand<TParameter, ISpecification<TParameter>>, IApplication
	{
		readonly IFactory<ApplicationExecutionParameter<TParameter>, ICommand[]> commandFactory;

		protected Application( [Required]Assembly[] assemblies, IEnumerable<ICommand> commands ) : this( assemblies, new ApplicationCommandFactory<TParameter>( commands ) ) {}

		protected Application( [Required]Assembly[] assemblies, [Required]IFactory<ApplicationExecutionParameter<TParameter>, ICommand[]> commandFactory ) : base( new WrappedSpecification<TParameter>( new OnlyOnceSpecification() ) )
		{
			Assemblies = assemblies;
			this.commandFactory = commandFactory;
		}

		[Required]
		public Assembly[] Assemblies { [return: Required]get; set; }

		protected override IEnumerable<ICommand> DetermineCommands( TParameter parameter )
		{
			var context = new ApplicationExecutionParameter<TParameter>( this, parameter );
			var result = commandFactory.Create( context ).Concat( base.DetermineCommands( parameter ) ).ToArray();
			return result;
		}
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
