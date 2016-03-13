using DragonSpark.Activation.FactoryModel;
using DragonSpark.ComponentModel;
using DragonSpark.Composition;
using DragonSpark.Extensions;
using DragonSpark.Runtime;
using DragonSpark.Runtime.Specifications;
using DragonSpark.Runtime.Values;
using Microsoft.Practices.ServiceLocation;
using PostSharp.Patterns.Contracts;
using System;
using System.Collections.Generic;
using System.Composition.Hosting;
using System.Linq;
using System.Reflection;
using System.Windows.Input;
using DragonSpark.TypeSystem;

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

	/*public class ApplicationConfiguration
	{
		readonly Assembly[] assemblies;

		public ApplicationConfiguration( Assembly[] assemblies )
		{
			this.assemblies = assemblies;
		}
	}

	public class ApplicationFactory : FactoryBase<IApplication>
	{
		protected override IApplication CreateItem()
		{
			throw new NotImplementedException();
		}
	}*/

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

	public interface IApplication : ICommand
	{
		/*CompositionHost Host { get; }

		IServiceLocator Locator { get; }*/

		Assembly[] Assemblies { get; }
	}

	public abstract class Application<TParameter> : CompositeCommand<TParameter, ISpecification<TParameter>>, IApplication
	{
		protected Application( [Required]Assembly[] assemblies, IEnumerable<ICommand> commands ) 
			: base( new WrappedSpecification<TParameter>( new OnlyOnceSpecification() ), commands.ToArray() )
		{
			Assemblies = assemblies;

			var core = new ICommand[]
			{
				new FixedCommand( new AssignApplication(), () => Assemblies ),
				new FixedCommand( new AmbientContextCommand<ITaskMonitor>(), () => new TaskMonitor() )
			};

			core.Each( Commands.Insert );
		}

		[Required]
		public Assembly[] Assemblies { [return: Required]get; set; }
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

	public class Setup : CompositeCommand, ISetup
	{
		public Setup( params ICommand[] commands ) : base( commands ) {}

		public Collection<object> Items { get; } = new Collection<object>();
	}
}
