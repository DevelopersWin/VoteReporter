﻿using DragonSpark.Activation;
using DragonSpark.Aspects;
using DragonSpark.ComponentModel;
using DragonSpark.Composition;
using DragonSpark.Extensions;
using DragonSpark.Properties;
using DragonSpark.Runtime;
using DragonSpark.Runtime.Specifications;
using DragonSpark.Runtime.Values;
using DragonSpark.TypeSystem;
using PostSharp.Patterns.Contracts;
using Serilog;
using System;
using System.Collections.Generic;
using System.Composition;
using System.Composition.Hosting;
using System.Linq;
using System.Reflection;
using System.Windows.Input;
using Type = System.Type;

namespace DragonSpark.Setup
{
	public class ApplicationServiceProviderFactory : ConfiguredServiceProviderFactory<ConfigureProviderCommand>
	{
		public ApplicationServiceProviderFactory( [Required] Type[] types ) : this( new Func<ContainerConfiguration>( new TypeBasedConfigurationContainerFactory( types ).Create ) ) {}

		public ApplicationServiceProviderFactory( [Required] Assembly[] assemblies ) : this( new Func<ContainerConfiguration>( new AssemblyBasedConfigurationContainerFactory( assemblies ).Create ) ) {}

		public ApplicationServiceProviderFactory( [Required] Func<ContainerConfiguration> source ) : this( new Func<IServiceProvider>( new ServiceProviderFactory( source ).Create ) ) {}

		public ApplicationServiceProviderFactory( Func<IServiceProvider> provider ) : base( provider ) {}
	}

	public sealed class ConfigureProviderCommand : Command<IServiceProvider>
	{
		readonly ILogger logger;
		readonly IServiceProviderHost host;

		public ConfigureProviderCommand( [Required]ILogger logger, [Required]IServiceProviderHost host )
		{
			this.logger = logger;
			this.host = host;
		}

		protected override void OnExecute( IServiceProvider parameter )
		{
			logger.Information( Resources.ConfiguringServiceLocatorSingleton );

			var assign = new AssignValueCommand<IServiceProvider>( host ).ExecuteWith( parameter );
			parameter.Get<IDisposableRepository>().With( repository => repository.Add( assign ) );
		}
	}

	public class AssignServiceProvider : AssignValueCommand<IServiceProvider>
	{
		// public AssignServiceProvider() : this( null ) {}

		public AssignServiceProvider( IServiceProvider current ) : this( CurrentServiceProvider.Instance, current ) {}

		public AssignServiceProvider( IWritableValue<IServiceProvider> value, IServiceProvider current ) : base( value, current ) {}

		/*protected override void OnExecute( IServiceProvider parameter )
		{
			var current = 
			base.OnExecute( parameter );
		}*/
	}

	public static class ApplicationExtensions
	{
		public static IApplication<T> Run<T>( this IApplication<T> @this, T arguments )
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
			application.Get<IDisposableRepository>().With( application.AssociateForDispose );
		}

		protected override void OnDispose()
		{
			application.Dispose();
			assign.Dispose();
		}
	}

	public class DefaultServiceProvider : ExecutionContextValue<ServiceProvider>
	{
		public static DefaultServiceProvider Instance { get; } = new DefaultServiceProvider( () => new ServiceProvider() );

		DefaultServiceProvider( Func<ServiceProvider> create ) : base( create ) {}
	}

	public class CurrentServiceProvider : ExecutionContextValue<IServiceProvider>
	{
		public static CurrentServiceProvider Instance { get; } = new CurrentServiceProvider();

		CurrentServiceProvider() {}

		public override IServiceProvider Item => base.Item ?? DefaultServiceProvider.Instance.Item;

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
		public CompositeServiceProvider( [Required] params IServiceProvider[] providers ) 
			: base( providers.Select( provider => new Func<Type, object>( new RecursionAwareServiceProvider( provider ).GetService ) ).ToArray() ) {}

		public object GetService( Type serviceType ) => serviceType == typeof(IServiceProvider) ? this : Create( serviceType );
	}

	public class RecursionAwareServiceProvider : DecoratedServiceProvider
	{
		public RecursionAwareServiceProvider( IServiceProvider inner ) : base( inner ) {}

		public override object GetService( Type serviceType )
		{
			var context = new IsActive( this, serviceType );
			if ( !context.Item )
			{
				using ( new AssignValueCommand<bool>( context ).ExecuteWith( true ) )
				{
					return base.GetService( serviceType );
				}
			}

			return null;
		}

		class IsActive : ThreadAmbientValue<bool>
		{
			public IsActive( object owner, Type type ) : base( KeyFactory.Instance.CreateUsing( owner, type ).ToString() ) {}
		}
	}

	public class DecoratedServiceProvider : IServiceProvider
	{
		readonly Func<Type, object> inner;

		public DecoratedServiceProvider( IServiceProvider provider ) : this( provider.GetService ) {}

		public DecoratedServiceProvider( [Required] Func<Type, object> inner )
		{
			this.inner = inner;
		}

		public virtual object GetService( Type serviceType ) => inner( serviceType );
	}

	public class ConfiguredServiceProviderFactory<TCommand> : ConfiguringFactory<IServiceProvider> where TCommand : class, ICommand<IServiceProvider>
	{
		public ConfiguredServiceProviderFactory( [Required] Func<IServiceProvider> provider ) : base( provider, Configure<TCommand>.Instance.Run ) {}
	}

	class Configure<T> : Command<IServiceProvider> where T : class, ICommand<IServiceProvider>
	{
		public static Configure<T> Instance { get; } = new Configure<T>();

		protected override void OnExecute( IServiceProvider parameter ) => parameter.Get<T>().ExecuteWith( parameter );
	}

	public interface IApplication<in T> : IApplication, ICommand<T> {}

	public interface IApplication : ICommand, IServiceProvider, IDisposable
	{
		// void Register( IDisposable disposable );
	}

	/*public class FrameworkTypes : FactoryBase<Type[]>
	{
		public static FrameworkTypes Instance { get; } = new FrameworkTypes();

		[Freeze]
		protected override Type[] CreateItem() => new[] { typeof(ConfigureProviderCommand) };
	}*/

	public class SharedFrameworkTypes : FactoryBase<Type[]>
	{
		public static SharedFrameworkTypes Instance { get; } = new SharedFrameworkTypes();

		[Freeze]
		protected override Type[] CreateItem() => new[] { typeof(ConfigureProviderCommand), typeof(ParameterInfoFactoryTypeLocator), typeof(MemberInfoFactoryTypeLocator), typeof(ApplicationAssemblyLocator) };
	}

	public abstract class Application<TParameter> : CompositeCommand<TParameter>, IApplication<TParameter>
	{
		protected Application( [Required]IServiceProvider provider ) : this( provider, Default<ICommand>.Items ) {}

		protected Application( [Required]IServiceProvider provider, IEnumerable<ICommand> commands ) : this( commands )
		{
			Services = provider;
		}

		protected Application( IEnumerable<ICommand> commands ) : base( new OnlyOnceSpecification().Wrap<TParameter>(), commands.ToArray() ) {}

		[Required]
		public IServiceProvider Services { [return: Required]get; set; }

		public virtual object GetService( Type serviceType ) => typeof(IApplication).Adapt().IsAssignableFrom( serviceType ) ? this : Services.GetService( serviceType );
	}

	public class ApplyExportedCommandsCommand<T> : DisposingCommand<object> where T : ICommand
	{
		[Required, Service]
		public CompositionContext Host { [return: Required]get; set; }

		public string ContractName { get; set; }

		readonly ICollection<T> watching = new Collection<T>();

		protected override void OnExecute( object parameter )
		{
			var exports = Host.GetExports<T>( ContractName ).Fixed();
			watching.AddRange( exports );

			exports
				.Prioritize()
				.Each( setup => setup.ExecuteWith( parameter ) );
		}

		protected override void OnDispose()
		{
			watching.Purge().OfType<IDisposable>().Each( obj => obj.Dispose() );
			base.OnDispose();
		}
	}

	public class ApplyTaskMonitorCommand : FixedCommand
	{
		public ApplyTaskMonitorCommand() : base( new AmbientContextCommand<ITaskMonitor>(), new TaskMonitor() ) {}
	}

	public class ApplySetup : ApplyExportedCommandsCommand<ISetup> {}

	public interface ISetup : ICommand<object> {}

	public class Setup : CompositeCommand, ISetup
	{
		public Setup() : this( Default<ICommand>.Items ) {}

		public Setup( params ICommand[] commands ) : base( commands ) {}

		public Collection<object> Items { get; } = new Collection<object>();
	}
}
