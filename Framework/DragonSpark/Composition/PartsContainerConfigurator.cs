using DragonSpark.Activation;
using DragonSpark.Activation.IoC;
using DragonSpark.Extensions;
using DragonSpark.Runtime.Values;
using DragonSpark.Setup;
using DragonSpark.TypeSystem;
using PostSharp.Patterns.Contracts;
using System;
using System.Collections.Generic;
using System.Composition.Convention;
using System.Composition.Hosting;
using System.Composition.Hosting.Core;
using System.Linq;
using System.Reflection;

namespace DragonSpark.Composition
{
	public class CompositionFactory : FactoryBase<CompositionHost>
	{
		readonly Func<ContainerConfiguration> configuration;
		
		public CompositionFactory( [Required] Func<ContainerConfiguration> configuration )
		{
			this.configuration = configuration;
		}

		protected override CompositionHost CreateItem() => configuration().CreateContainer();
	}

	public class AssemblyBasedConfigurationContainerFactory : ContainerConfigurationFromPartsFactory
	{
		public AssemblyBasedConfigurationContainerFactory( [Required] Assembly[] assemblies, [Required] params ITransformer<ContainerConfiguration>[] configurations )
			: base( assemblies, TypesFactory.Instance.Create( assemblies ), configurations ) {}
	}

	public class TypeBasedConfigurationContainerFactory : ContainerConfigurationFromPartsFactory
	{
		public TypeBasedConfigurationContainerFactory( [Required] Type[] types, [Required] params ITransformer<ContainerConfiguration>[] configurations ) 
			: base( Default<Assembly>.Items, types, configurations ) {}
	}

	public class ContainerConfigurationFromPartsFactory : AggregateFactory<ContainerConfiguration>
	{
		public ContainerConfigurationFromPartsFactory( [Required] Assembly[] assemblies, [Required] Type[] types, params ITransformer<ContainerConfiguration>[] configurations )
			: base( 
				ContainerConfigurationFactory.Instance, 
				new ContainerServicesConfigurator().Append( configurations ).Append( new PartsContainerConfigurator( assemblies, types ) ).ToArray()
			) {}
	}

	public class ContainerConfigurationFactory : FactoryBase<ContainerConfiguration>
	{
		public static ContainerConfigurationFactory Instance { get; } = new ContainerConfigurationFactory();

		protected override ContainerConfiguration CreateItem() => new ContainerConfiguration();
	}

	public class ContainerServicesConfigurator : ContainerConfigurator
	{
		protected override ContainerConfiguration CreateItem( ContainerConfiguration parameter ) => parameter.WithProvider( new ServicesExportDescriptorProvider() );
	}

	public abstract class ContainerConfigurator : TransformerBase<ContainerConfiguration> {}

	public interface IServiceProviderHost : IWritableValue<IServiceProvider> {}

	class ServiceProviderHost : FixedValue<IServiceProvider>, IServiceProviderHost
	{
		public ServiceProviderHost()
		{
			Assign( Services.Current );
		}
	}

	public class ServicesExportDescriptorProvider : InstanceExportDescriptorProvider<IServiceProviderHost>
	{
		readonly IServiceProviderHost host;

		public ServicesExportDescriptorProvider() : this( new ServiceProviderHost() ) {}

		public ServicesExportDescriptorProvider( [Required]IServiceProviderHost host ) : base( host )
		{
			this.host = host;
		}

		public override IEnumerable<ExportDescriptorPromise> GetExportDescriptors( CompositionContract contract, DependencyAccessor descriptorAccessor )
		{
			var exportDescriptorPromises = base.GetExportDescriptors( contract, descriptorAccessor ).AnyOr( () => DeterminePromise( contract, descriptorAccessor ) );
			return exportDescriptorPromises;
		}

		IEnumerable<ExportDescriptorPromise> DeterminePromise( CompositionContract contract, DependencyAccessor descriptorAccessor )
		{
			CompositionDependency dependency;
			if ( !descriptorAccessor.TryResolveOptionalDependency( "Existing Request", contract, true, out dependency ) )
			{
				yield return new ExportDescriptorPromise( contract, GetType().FullName, true, NoDependencies, new Context( host.Item, contract ).Create );
			}
		}

		class Context
		{
			readonly IServiceProvider provider;
			readonly CompositionContract contract;

			public Context( [Required]IServiceProvider provider, [Required]CompositionContract contract )
			{
				this.provider = provider;
				this.contract = contract;
			}

			public ExportDescriptor Create( IEnumerable<CompositionDependency> dependencies ) => ExportDescriptor.Create( Activate, NoMetadata );

			object Activate( LifetimeContext context, CompositionOperation operation ) => provider.GetService( contract.ContractType );
		}
	}

	public class PartsContainerConfigurator : ContainerConfigurator
	{
		readonly Assembly[] assemblies;
		readonly Type[] types;
		readonly Type[] core;

		public PartsContainerConfigurator( [Required] Assembly[] assemblies, [Required]Type[] types ) : this( assemblies, types, FrameworkTypes.Instance.Create() ) {}

		public PartsContainerConfigurator( [Required] Assembly[] assemblies, [Required]Type[] types, [Required] Type[] core )
		{
			this.assemblies = assemblies;
			this.types = types;
			this.core = core;
		}

		protected override ContainerConfiguration CreateItem( ContainerConfiguration configuration )
		{
			var factoryTypes = types.Where( FactoryTypeFactory.Specification.Instance.IsSatisfiedBy ).Select( FactoryTypeFactory.Instance.Create ).ToArray();
			var locator = new FactoryTypeRequestLocator( factoryTypes );
			var conventionLocator = new BuildableTypeFromConventionLocator( types );
			var activator = new Activation.Activator( conventionLocator );

			var result = configuration
				.WithParts( core, new ConventionBuilder().WithSelf( builder => builder.ForTypesMatching( type => true ).Export() ) )
				.WithParts( types, AttributeProvider.Instance )
				.WithInstance( assemblies )
				.WithInstance( types )
				.WithInstance( conventionLocator )
				.WithInstance( factoryTypes )
				.WithInstance( locator )
				.WithInstance<IActivator>( activator )
				.WithProvider( new FactoryDelegateExportDescriptorProvider( locator ) )
				.WithProvider( new FactoryWithParameterDelegateExportDescriptorProvider( locator ) )
				.WithProvider( new FactoryExportDescriptorProvider( locator ) )
				.WithProvider( new TypeInitializingExportDescriptorProvider( conventionLocator ) );
			return result;
		}

		class AttributeProvider : AttributedModelProvider
		{
			public static AttributeProvider Instance { get; } = new AttributeProvider();

			// [Freeze]
			public override IEnumerable<Attribute> GetCustomAttributes( Type reflectedType, MemberInfo member ) => member.GetAttributes<Attribute>();

			// [Freeze]
			public override IEnumerable<Attribute> GetCustomAttributes( Type reflectedType, ParameterInfo parameter ) => parameter.GetAttributes<Attribute>();
		}
	}
}