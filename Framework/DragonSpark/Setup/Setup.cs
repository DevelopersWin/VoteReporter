using DragonSpark.Activation;
using DragonSpark.ComponentModel;
using DragonSpark.Extensions;
using DragonSpark.Runtime;
using DragonSpark.Runtime.Specifications;
using DragonSpark.Runtime.Values;
using DragonSpark.TypeSystem;
using PostSharp.Patterns.Contracts;
using System;
using System.Collections.Generic;
using System.Composition;
using System.Linq;
using System.Windows.Input;
using Type = System.Type;

namespace DragonSpark.Setup
{
	public class AssignServiceProvider : AssignValueCommand<IServiceProvider>
	{
		public AssignServiceProvider() : this( null ) {}

		public AssignServiceProvider( IServiceProvider current ) : this( new CurrentServiceProvider(), current ) {}

		public AssignServiceProvider( IWritableValue<IServiceProvider> value, IServiceProvider current ) : base( value, current ) {}

		/*protected override void OnExecute( IServiceProvider parameter )
		{
			var current = 
			base.OnExecute( parameter );
		}*/
	}

	public static class ApplicationExtensions
	{
		public static IApplication Run<T>( this IApplication<T> @this, T arguments )
		{
			using ( var command = new ExecuteApplicationCommand<T>( @this ) )
			{
				command.ExecuteWith( arguments );
			}
			return @this;
		}
	}

	public class ExecuteApplicationCommand<T> : DisposingCommand<T>
	{
		readonly IApplication<T> application;
		readonly AssignServiceProvider assign;

		public ExecuteApplicationCommand( [Required]IApplication<T> application, IServiceProvider current = null ) : this( application, new AssignServiceProvider( current ) ) {}

		public ExecuteApplicationCommand( [Required]IApplication<T> application, AssignServiceProvider assign )
		{
			this.application = application;
			this.assign = assign;
		}

		protected override void OnExecute( T parameter )
		{
			assign.ExecuteWith( application );
			application.ExecuteWith<ICommand>( parameter );
		}

		protected override void OnDispose()
		{
			assign.Dispose();
			base.OnDispose();
		}

		// protected override void OnDispose() => application.Get<AutoData>().Dispose();
	}

	/*public class ExecuteApplicationCommand : DisposingCommand<IApplication>
	{
		public ExecuteApplicationCommand( IWritableValue<> )
		{
		}

		protected override void OnExecute( IApplication parameter )
		{
		}
	}*/

	public class CurrentServiceProvider : ExecutionContextValue<IServiceProvider>
	{
		public override void Assign( IServiceProvider item )
		{
			if ( Item != null && item != null && Item != item )
			{
				
			}

			base.Assign( item );
		}
	}

	public class InstanceServiceProvider : Collection<object>, IServiceProvider
	{
		public InstanceServiceProvider( [Required] params object[] instances )
		{
			this.AddRange( instances );
		}

		public object GetService( Type serviceType ) => this.FirstOrDefault( serviceType.Adapt().IsInstanceOfType );
	}

	public class CompositeServiceProvider : FirstFromParameterFactory<Type, object>, IServiceProvider
	{
		public CompositeServiceProvider( params IServiceProvider[] locators ) 
			: base( locators.Select( activator => new Func<Type, object>( activator.GetService ) ).ToArray() ) {}

		public object GetService( Type serviceType ) => serviceType == typeof(IServiceProvider) ? this : Create( serviceType );

		protected override object DetermineFirst( IEnumerable<Func<Type, object>> factories, Type parameter )
		{
			var result = factories.Where( func => !new IsActive( func ).Item ).FirstWhere( factory =>
			{
				using ( new AssignValueCommand<bool>( new IsActive( factory ) ).ExecuteWith( true ) )
				{
					var o = factory( parameter );
					return o;
				}
			} );
			return result;
		}

		class IsActive : AssociatedValue<object, bool>
		{
			public IsActive( object instance ) : base( instance ) {}
		}
	}

	/*public class ServiceProviderFactory : FactoryBase<IServiceProvider>
	{
		readonly Func<IServiceProvider[]> providers;

		public ServiceProviderFactory( [Required] Func<IServiceProvider[]> providers )
		{
			this.providers = providers;
		}

		protected override IServiceProvider CreateItem() => new CompositeServiceProvider( providers().Fixed() );
	}*/


	public class ServiceProviderFactory<TCommand> : ServiceProviderFactory<TCommand, IServiceProvider> where TCommand : class, ICommand<IServiceProvider>
	{
		public ServiceProviderFactory( [Required] Func<IServiceProvider> provider ) : base( provider ) {}
	}
	
	public class ServiceProviderFactory<TCommand, TProvider> : ConfiguringFactory<IServiceProvider> where TCommand : class, ICommand<TProvider> where TProvider : class, IServiceProvider
	{
		public ServiceProviderFactory( [Required] Func<IServiceProvider> provider ) : base( provider, Configure.Instance.Run ) {}

		class Configure : Command<IServiceProvider>
		{
			public static Configure Instance { get; } = new Configure();

			protected override void OnExecute( IServiceProvider parameter )
			{
				var provider = parameter as TProvider ?? parameter.Get<TProvider>();
				var command = parameter.Get<TCommand>();
				command.ExecuteWith( provider );
			}
		}
	}

	/*public abstract class ConfigureProviderCommandBase<T> : Command<IServiceProvider> where T : class
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
	}*/

	public interface IApplication<in T> : IApplication, ICommand<T> {}
	
	public interface IApplication : ICommand, IServiceProvider, IDisposable {}

	/*public class ApplicationCommandFactory : FactoryBase<IApplication, IEnumerable<ICommand>>
	{
		public static ApplicationCommandFactory Instance { get; } = new ApplicationCommandFactory();

		protected override IEnumerable<ICommand> CreateItem( IApplication parameter ) => new ICommand[]
		{
			new FixedCommand( new AssignServiceProvider(), () => parameter ),
			new FixedCommand( new AmbientContextCommand<ITaskMonitor>(), () => new TaskMonitor() ) // TODO: Move?
		};
	}*/

	public abstract class Application<TParameter> : CompositeCommand<TParameter, ISpecification<TParameter>>, IApplication<TParameter>
	{
		protected Application( [Required]IServiceProvider provider ) : this( provider, Default<ICommand>.Items ) {}

		protected Application( [Required]IServiceProvider provider, IEnumerable<ICommand> commands ) : this( commands )
		{
			Services = provider;
		}

		protected Application( IEnumerable<ICommand> commands ) : base( new OnlyOnceSpecification().Wrap<TParameter>(), commands.ToArray() ) {}

		/*/*[Default( true )]
		public bool DisposeAfterExecution { get; set; }#1#

		protected override void OnExecute( TParameter parameter )
		{
			// ApplicationCommandFactory.Instance.Create( this ).Each( Commands.Insert );

			base.OnExecute( parameter );

			// DisposeAfterExecution.IsTrue( Dispose );
		}*/

		[Required]
		public IServiceProvider Services { [return: Required]get; set; }

		public void Dispose()
		{
			Dispose( true );
			GC.SuppressFinalize( this );
		}

		void Dispose( bool disposing ) => disposing.IsTrue( OnDispose );

		// [Freeze]
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
