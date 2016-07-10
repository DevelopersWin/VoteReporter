using DragonSpark.Activation;
using DragonSpark.Activation.IoC;
using DragonSpark.Aspects;
using DragonSpark.Extensions;
using DragonSpark.Runtime.Properties;
using DragonSpark.Runtime.Stores;
using DragonSpark.Setup;
using DragonSpark.TypeSystem;
using PostSharp.Patterns.Contracts;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition.Convention;
using System.Composition.Hosting;
using System.Composition.Hosting.Core;
using System.Linq;
using System.Reflection;
using CompositeActivator = System.Composition.Hosting.Core.CompositeActivator;

namespace DragonSpark.Composition
{
	public class CompositionFactory : FactoryBase<CompositionHost>
	{
		readonly IFactory<ContainerConfiguration> configuration;
		
		public CompositionFactory( [Required] IFactory<ContainerConfiguration> configuration )
		{
			this.configuration = configuration;
		}

		public override CompositionHost Create() => configuration.Create().CreateContainer();
	}

	public class AssemblyBasedConfigurationContainerFactory : ContainerConfigurationFromPartsFactory
	{
		public AssemblyBasedConfigurationContainerFactory( [Required] Assembly[] assemblies )
			: base( assemblies, TypesFactory.Instance.Create( assemblies ) ) {}
	}

	public class TypeBasedConfigurationContainerFactory : ContainerConfigurationFromPartsFactory
	{
		public TypeBasedConfigurationContainerFactory( Type[] types )  : base( Items<Assembly>.Default, types ) {}
	}

	public class ContainerConfigurationFromPartsFactory : AggregateFactory<ContainerConfiguration>
	{
		public ContainerConfigurationFromPartsFactory( [Required] Assembly[] assemblies, [Required] Type[] types )
			: base( ContainerConfigurationFactory.Instance.ToDelegate(), ContainerServicesConfigurator.Instance.ToDelegate(), new PartsContainerConfigurator( assemblies, types ).ToDelegate() ) {}
	}

	public class ContainerConfigurationFactory : FactoryBase<ContainerConfiguration>
	{
		public static ContainerConfigurationFactory Instance { get; } = new ContainerConfigurationFactory();
		ContainerConfigurationFactory() {}

		public override ContainerConfiguration Create() => new ContainerConfiguration();
	}

	public class ContainerServicesConfigurator : ContainerConfigurator
	{
		public static ContainerServicesConfigurator Instance { get; } = new ContainerServicesConfigurator();
		ContainerServicesConfigurator() {}

		public override ContainerConfiguration Create( ContainerConfiguration parameter ) => parameter.WithProvider( new ServicesExportDescriptorProvider() );
	}

	public abstract class ContainerConfigurator : TransformerBase<ContainerConfiguration> {}

	public interface IServiceProviderHost : IWritableStore<IServiceProvider> {}

	class ServiceProviderHost : FixedStore<IServiceProvider>, IServiceProviderHost
	{
		public ServiceProviderHost()
		{
			Assign( DefaultServiceProvider.Instance.Value );
		}
	}

	public class ServicesExportDescriptorProvider : InstanceExportDescriptorProvider<IServiceProviderHost>
	{
		readonly IServiceProviderHost host;
		readonly Func<Type, object> get;

		public ServicesExportDescriptorProvider() : this( new ServiceProviderHost() ) {}

		public ServicesExportDescriptorProvider( IServiceProviderHost host ) : base( host )
		{
			this.host = host;
			get = Get;
		}

		public override IEnumerable<ExportDescriptorPromise> GetExportDescriptors( CompositionContract contract, DependencyAccessor descriptorAccessor )
		{
			var promises = base.GetExportDescriptors( contract, descriptorAccessor ).Fixed();
			var result = promises.Any() ? promises : DeterminePromise( contract, descriptorAccessor );
			return result;
		}

		IEnumerable<ExportDescriptorPromise> DeterminePromise( CompositionContract contract, DependencyAccessor descriptorAccessor )
		{
			CompositionDependency dependency;
			if ( !descriptorAccessor.TryResolveOptionalDependency( "Existing Request", contract, true, out dependency ) )
			{
				yield return new ExportDescriptorPromise( contract, GetType().FullName, true, NoDependencies, new Factory( get, contract.ContractType ).Create );
			}
		}

		object Get( Type type ) => host.Value.GetService( type );

		class Factory : FixedFactory<Type, object>
		{
			readonly CompositeActivator activate;

			public Factory( Func<Type, object> provider, Type contract ) : base( provider, contract )
			{
				activate = Activate;
			}

			object Activate( LifetimeContext context, CompositionOperation operation ) => Create();

			public ExportDescriptor Create( IEnumerable<CompositionDependency> dependencies ) => ExportDescriptor.Create( activate, NoMetadata );
		}
	}

	public class PartsContainerConfigurator : ContainerConfigurator
	{
		readonly Assembly[] assemblies;
		readonly ImmutableArray<Type> core;
		readonly Type[] types, all;
		readonly FactoryTypeRequest[] factoryTypes;
		readonly Activation.FactoryTypeLocator locator;
		readonly BuildableTypeFromConventionLocator conventionLocator;
		readonly Activation.Activator activator;

		public PartsContainerConfigurator( Assembly[] assemblies, Type[] types ) : this( assemblies, types, FrameworkTypes.Instance.Value ) {}

		public PartsContainerConfigurator( Assembly[] assemblies, Type[] types, ImmutableArray<Type> core )
		{
			this.assemblies = assemblies;
			this.types = types;
			this.core = core;

			// TODO: Fix this mess:
			factoryTypes = FactoryTypeLocator.Instance.GetMany( types );
			locator = new Activation.FactoryTypeLocator( factoryTypes );
			conventionLocator = new BuildableTypeFromConventionLocator( types );
			activator = new Activation.Activator( conventionLocator );
			all = types.Union( core.ToArray() ).Fixed();
		}

		public override ContainerConfiguration Create( ContainerConfiguration configuration )
		{
			var result = configuration
				.WithInstance( assemblies )
				.WithInstance( all )
				.WithInstance( conventionLocator )
				.WithInstance( factoryTypes )
				.WithInstance( locator )
				.WithInstance<IActivator>( activator )
				// .WithParts( shared, SharedFrameworkConventionBuilder.Instance.Create() )
				.WithParts( core, FrameworkConventionBuilder.Instance.Create() )
				.WithParts( types, AttributeProvider.Instance )
				.WithProvider( new FactoryDelegateExportDescriptorProvider( locator ) )
				.WithProvider( new FactoryWithParameterDelegateExportDescriptorProvider( locator ) )
				.WithProvider( new FactoryExportDescriptorProvider( locator ) )
				.WithProvider( new TypeInitializingExportDescriptorProvider( conventionLocator ) );
			return result;
		}

		/*class SharedFrameworkConventionBuilder : FrameworkConventionBuilder
		{
			public new static SharedFrameworkConventionBuilder Instance { get; } = new SharedFrameworkConventionBuilder();

			protected override PartConventionBuilder Configure( ConventionBuilder builder ) => base.Configure( builder ).Shared();
		}*/

		public class FrameworkConventionBuilder : FactoryBase<ConventionBuilder>
		{
			public static FrameworkConventionBuilder Instance { get; } = new FrameworkConventionBuilder();

			public override ConventionBuilder Create() => new ConventionBuilder().WithSelf( Configure );

			protected virtual PartConventionBuilder Configure( ConventionBuilder builder ) => builder.ForTypesMatching( type => true ).Export().SelectConstructor( infos => infos.First(), ( info, conventionBuilder ) => conventionBuilder.AsMany( false ) );
		}

		class AttributeProvider : AttributedModelProvider
		{
			public static AttributeProvider Instance { get; } = new AttributeProvider();

			[Freeze]
			public override IEnumerable<Attribute> GetCustomAttributes( Type reflectedType, MemberInfo member ) => member.GetAttributes<Attribute>();

			[Freeze]
			public override IEnumerable<Attribute> GetCustomAttributes( Type reflectedType, ParameterInfo parameter ) => parameter.GetAttributes<Attribute>();
		}
	}
}