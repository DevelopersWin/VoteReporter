using DragonSpark.Activation;
using DragonSpark.Activation.IoC;
using DragonSpark.Aspects;
using DragonSpark.Diagnostics;
using DragonSpark.Extensions;
using DragonSpark.Properties;
using DragonSpark.Runtime;
using DragonSpark.Runtime.Specifications;
using DragonSpark.Setup;
using DragonSpark.TypeSystem;
using Microsoft.Practices.ServiceLocation;
using PostSharp.Patterns.Contracts;
using Serilog;
using System;
using System.Collections.Generic;
using System.Composition;
using System.Composition.Hosting;
using System.Reflection;
using Type = System.Type;

namespace DragonSpark.Composition
{
	public class FactoryTypeFactory : FactoryBase<Type, FactoryTypeRequest>
	{
		public static FactoryTypeFactory Instance { get; } = new FactoryTypeFactory( Specification.Instance );

		public FactoryTypeFactory( ISpecification<Type> specification ) : base( specification ) {}

		public class Specification : CanBuildSpecification
		{
			public new static Specification Instance { get; } = new Specification();

			[Freeze]
			protected override bool Verify( Type parameter ) => base.Verify( parameter ) && Factory.IsFactory( parameter ) && parameter.Adapt().IsDefined<ExportAttribute>();
		}

		protected override FactoryTypeRequest CreateItem( Type parameter ) => new FactoryTypeRequest( parameter, parameter.From<ExportAttribute, string>( attribute => attribute.ContractName ), Factory.GetResultType( parameter ) );
	}

	public class ConfiguredServiceProviderFactory : ConfiguredServiceProviderFactory<ConfigureProviderCommand>
	{
		public ConfiguredServiceProviderFactory( [Required] Type[] types ) : this( new Func<ContainerConfiguration>( new TypeBasedConfigurationContainerFactory( types ).Create ) ) {}

		public ConfiguredServiceProviderFactory( [Required] Assembly[] assemblies ) : this( new Func<ContainerConfiguration>( new AssemblyBasedConfigurationContainerFactory( assemblies ).Create ) ) {}

		public ConfiguredServiceProviderFactory( [Required] Func<ContainerConfiguration> source ) : this( new Func<IServiceProvider>( new ServiceProviderFactory( source ).Create ) ) {}

		public ConfiguredServiceProviderFactory( Func<IServiceProvider> provider ) : base( provider ) {}
	}

	public class ServiceProviderFactory : FactoryBase<IServiceProvider>
	{
		readonly Func<CompositionContext> source;

		public ServiceProviderFactory( [Required] Assembly[] assemblies ) : this( new Func<ContainerConfiguration>( new AssemblyBasedConfigurationContainerFactory( assemblies ).Create ) ) {}

		public ServiceProviderFactory( Func<ContainerConfiguration> configuration ) : this( new Func<CompositionContext>( new CompositionFactory( configuration ).Create ) ) {}

		public ServiceProviderFactory( [Required] Func<CompositionContext> source )
		{
			this.source = source;
		}

		protected override IServiceProvider CreateItem()
		{
			var context = source();
			var primary = new ServiceLocator( context );
			var result = new CompositeServiceProvider( new InstanceServiceProvider( context, primary ), primary, DefaultServiceProvider.Instance.Item );
			return result;
		}
	}

	public sealed class ConfigureProviderCommand : Command<IServiceProvider>
	{
		readonly ILogger logger;
		readonly IServiceProviderHost host;
		readonly IDisposableRepository repository;

		public ConfigureProviderCommand( [Required]ILogger logger, [Required]IServiceProviderHost host, [Required] IDisposableRepository repository )
		{
			this.logger = logger;
			this.host = host;
			this.repository = repository;
		}

		protected override void OnExecute( IServiceProvider parameter )
		{
			logger.Information( Resources.ConfiguringServiceLocatorSingleton );

			repository.Add( new AssignValueCommand<IServiceProvider>( host ).ExecuteWith( parameter ) );
		}
	}

	public class ServiceLocator : ServiceLocatorImplBase
	{
		readonly CompositionContext host;

		public ServiceLocator( [Required]CompositionContext host )
		{
			this.host = host;
		}

		protected override IEnumerable<object> DoGetAllInstances(Type serviceType) => host.GetExports( serviceType, null );

		protected override object DoGetInstance(Type serviceType, string key) => host.TryGet<object>( serviceType, key );
	}
}
