using DragonSpark.ComponentModel;
using DragonSpark.Composition;
using DragonSpark.Extensions;
using DragonSpark.Runtime;
using DragonSpark.Runtime.Values;
using DragonSpark.TypeSystem;
using PostSharp.Patterns.Contracts;
using System;
using System.Reflection;
using System.Windows.Input;

namespace DragonSpark.Setup
{
	public class ApplicationHost : CompositeWritableValue<Assembly[]>
	{
		public ApplicationHost() : this( new AssemblyHost(), new CompositionHost() ) {}

		public ApplicationHost( params IWritableValue<Assembly[]>[] values ) : base( values ) {}
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
			var host =  factory( item );
			context.Assign( host );

			base.Assign( item );
		}
	}

	public abstract class Application<TArguments> : SetupContainerBase<TArguments>
	{
		protected Application( params ICommand<TArguments>[] commands ) : base( commands ) {}

		public Assembly[] Assemblies { get; set; }

		protected override void OnExecute( TArguments parameter )
		{
			var assemblies = Assemblies ?? new AssemblyHost().Item;
			using ( new AssignValueCommand<Assembly[]>( new ApplicationHost() ).ExecuteWith( assemblies ) )
			{
				using ( new AmbientContextCommand<ITaskMonitor>().ExecuteWith( new TaskMonitor() ) )
				{
					base.OnExecute( parameter );
				}
			}
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
