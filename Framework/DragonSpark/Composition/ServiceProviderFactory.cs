using DragonSpark.Activation;
using DragonSpark.Activation.IoC;
using DragonSpark.Aspects;
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

	public class ServiceProviderFactory : ServiceProviderFactory<ConfigureProviderCommand>
	{
		public ServiceProviderFactory( [Required] Type[] types ) : this( new Func<ContainerConfiguration>( new TypeBasedConfigurationContainerFactory( types ).Create ) ) {}

		public ServiceProviderFactory( [Required] Assembly[] assemblies ) : this( new Func<ContainerConfiguration>( new AssemblyBasedConfigurationContainerFactory( assemblies ).Create ) ) {}

		public ServiceProviderFactory( [Required] Func<ContainerConfiguration> source ) : this( new Func<IServiceProvider>( new ServiceProviderCoreFactory( source ).Create ) ) {}

		public ServiceProviderFactory( Func<IServiceProvider> provider ) : base( provider ) {}
	}

	public class ServiceProviderCoreFactory : FactoryBase<IServiceProvider>
	{
		readonly Func<CompositionContext> source;

		public ServiceProviderCoreFactory( Func<ContainerConfiguration> configuration ) : this( new Func<CompositionContext>( new CompositionFactory( configuration ).Create ) ) {}

		public ServiceProviderCoreFactory( [Required] Func<CompositionContext> source )
		{
			this.source = source;
		}

		protected override IServiceProvider CreateItem()
		{
			var context = source();
			var primary = new ServiceLocator( context );
			var result = new CompositeServiceProvider( new InstanceServiceProvider( context, primary ), primary, Services.Current );
			return result;
		}
	}

	// [Export]
	public sealed class ConfigureProviderCommand : Command<IServiceProvider>
	{
		readonly ILogger logger;
		readonly CompositionContext context;

		// [ImportingConstructor]
		public ConfigureProviderCommand( [Required]ILogger logger, [Required]CompositionContext context )
		{
			this.logger = logger;
			this.context = context;
		}

		protected override void OnExecute( IServiceProvider parameter )
		{
			logger.Information( Resources.ConfiguringServiceLocatorSingleton );

			var host = context.TryGet<IServiceProviderHost>();
			if ( host == null )
			{
				logger.Warning( $"The {nameof( IServiceProviderHost )} is not registered for this {nameof( CompositionContext )}." );
			}
			else
			{
				host.Assign( parameter );
			}
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
