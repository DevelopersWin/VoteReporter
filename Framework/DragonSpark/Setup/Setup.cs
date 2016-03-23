using DragonSpark.Activation;
using DragonSpark.Activation.FactoryModel;
using DragonSpark.Aspects;
using DragonSpark.ComponentModel;
using DragonSpark.Extensions;
using DragonSpark.Runtime;
using DragonSpark.Runtime.Specifications;
using DragonSpark.Runtime.Values;
using PostSharp.Patterns.Contracts;
using System;
using System.Collections.Generic;
using System.Composition;
using System.Linq;
using System.Windows.Input;
using Type = System.Type;

namespace DragonSpark.Setup
{
	public class AssignApplication : AssignValueCommand<IApplication>
	{
		public AssignApplication() : this( new CurrentApplication() ) {}

		public AssignApplication( IWritableValue<IApplication> value ) : base( value ) {}
	}

	public class CurrentApplication : ExecutionContextValue<IApplication> {}

	class ServiceProvider : IServiceProvider
	{
		readonly IServiceProvider locator;
		readonly IActivator activator;

		public ServiceProvider( [Required]IServiceProvider provider ) : this( provider, provider.Get<IActivator>() ) {}

		public ServiceProvider( [Required]IServiceProvider locator, [Required]IActivator activator )
		{
			this.locator = locator;
			this.activator = activator;
		}

		public object GetService( Type serviceType ) => locator.GetService( serviceType ) ?? activator.Activate<object>( serviceType );
	}

	public class ServiceProviderFactory : ConfiguringFactory<IServiceProvider>
	{
		public ServiceProviderFactory( Func<IServiceProvider> inner, Action<IServiceProvider> configure ) : base( inner, configure )
		{
		}

		protected override IServiceProvider CreateItem()
		{
			var configured = base.CreateItem();
			var result = new ServiceProvider( configured );
			return result;
		}
	}

	public abstract class ConfigureProviderCommandBase<T> : Command<IServiceProvider> where T : class
	{
		public class ProviderContext
		{
			public ProviderContext( [Required] IServiceProvider provider ) : this( provider, provider.Get<T>() ) {}

			public ProviderContext( [Required] IServiceProvider provider, [Required]T context )
			{
				Provider = provider;
				Context = context;
			}

			public IServiceProvider Provider { get; }
			public T Context { get; }
		}

		protected override void OnExecute( IServiceProvider parameter ) => new ProviderContext( parameter ).With( Configure );
		protected abstract void Configure( ProviderContext context );
	}

	public interface IApplication : ICommand, IServiceProvider, IDisposable {}

	public abstract class Application<TParameter> : CompositeCommand<TParameter, ISpecification<TParameter>>, IApplication
	{
		protected Application( [Required]IServiceProvider provider, IEnumerable<ICommand> commands ) : this( commands )
		{
			Services = provider;
		}

		protected Application( IEnumerable<ICommand> commands ) : base( AlwaysSpecification.Instance.Wrap<TParameter>(), commands.ToArray() ) {}

		[Default( true )]
		public bool DisposeAfterExecution { get; set; }

		protected override void OnExecute( TParameter parameter )
		{
			var core = new ICommand[]
			{
				new FixedCommand( new AssignApplication(), () => this ),
				new FixedCommand( new AmbientContextCommand<ITaskMonitor>(), () => new TaskMonitor() )
			};

			core.Each( Commands.Insert );

			base.OnExecute( parameter );

			DisposeAfterExecution.IsTrue( Dispose );
		}

		[Required]
		public IServiceProvider Services { [return: Required]get; set; }

		public void Dispose()
		{
			Dispose( true );
			GC.SuppressFinalize( this );
		}

		void Dispose( bool disposing ) => disposing.IsTrue( OnDispose );

		[Freeze]
		protected virtual void OnDispose()
		{
			Commands.OfType<IDisposable>().Reverse().Each( disposable => disposable.Dispose() );
			Commands.Clear();
		}

		public virtual object GetService( Type serviceType ) => typeof(IApplication).Adapt().IsAssignableFrom( serviceType ) ? this : Services.GetService( serviceType );
	}

	public class ApplyExportedCommandsCommand<T> : Command<object> where T : ICommand
	{
		[Required, Service]
		public CompositionContext Host { [return: Required]get; set; }

		public string ContractName { get; set; }

		protected override void OnExecute( object parameter ) => 
			Host.GetExports<T>( ContractName )
				.Prioritize()
				.Each( setup => setup.ExecuteWith( parameter ) );
	}

	public class ApplySetup : ApplyExportedCommandsCommand<ISetup> {}

	public interface ISetup : ICommand<object> {}

	public class Setup : CompositeCommand, ISetup
	{
		public Setup( params ICommand[] commands ) : base( commands ) {}

		public Collection<object> Items { get; } = new Collection<object>();
	}
}
