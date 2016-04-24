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
				configurations.Prepend( /*new DisposingConfigurator(),*/ new ContainerServicesConfigurator(), new PartsContainerConfigurator( assemblies, types ) ).ToArray()
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

		public ServicesExportDescriptorProvider() : this( new ServiceProviderHost() ) {}

		public ServicesExportDescriptorProvider( [Required]IServiceProviderHost host ) : base( host )
		{
			this.host = host;
		}

		public override IEnumerable<ExportDescriptorPromise> GetExportDescriptors( CompositionContract contract, DependencyAccessor descriptorAccessor ) => 
			base.GetExportDescriptors( contract, descriptorAccessor ).AnyOr( () => DeterminePromise( contract, descriptorAccessor ) );

		IEnumerable<ExportDescriptorPromise> DeterminePromise( CompositionContract contract, DependencyAccessor descriptorAccessor )
		{
			CompositionDependency dependency;
			if ( !descriptorAccessor.TryResolveOptionalDependency( "Existing Request", contract, true, out dependency ) )
			{
				yield return new ExportDescriptorPromise( contract, GetType().FullName, true, NoDependencies, new Context( Get, contract ).Create );
			}
		}

		object Get( Type type ) => host.Value.GetService( type );

		class Context
		{
			readonly Func<Type, object> provider;
			readonly CompositionContract contract;

			public Context( [Required]Func<Type, object> provider, [Required]CompositionContract contract )
			{
				this.provider = provider;
				this.contract = contract;
			}

			public ExportDescriptor Create( IEnumerable<CompositionDependency> dependencies ) => ExportDescriptor.Create( Activate, NoMetadata );

			object Activate( LifetimeContext context, CompositionOperation operation ) => provider( contract.ContractType );
		}
	}

	public class PartsContainerConfigurator : ContainerConfigurator
	{
		readonly Assembly[] assemblies;
		readonly Type[] types;
		readonly Type[] core;
		// readonly Type[] shared;

		public PartsContainerConfigurator( [Required] Assembly[] assemblies, [Required]Type[] types ) : this( assemblies, types, /*FrameworkTypes.Instance.Create(),*/ SharedFrameworkTypes.Instance.Create() ) {}

		public PartsContainerConfigurator( [Required] Assembly[] assemblies, [Required]Type[] types, [Required] Type[] core/*, [Required] Type[] shared*/ )
		{
			this.assemblies = assemblies;
			this.types = types;
			this.core = core;
			// this.shared = shared;
		}

		protected override ContainerConfiguration CreateItem( ContainerConfiguration configuration )
		{
			var factoryTypes = types.Where( FactoryTypeFactory.Specification.Instance.IsSatisfiedBy ).Select( FactoryTypeFactory.Instance.Create ).ToArray();
			var locator = new FactoryTypeRequestLocator( factoryTypes );
			var conventionLocator = new BuildableTypeFromConventionLocator( types );
			var activator = new Activation.Activator( conventionLocator );

			var result = configuration
				.WithInstance( assemblies )
				.WithInstance( types )
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

		class SharedFrameworkConventionBuilder : FrameworkConventionBuilder
		{
			public new static SharedFrameworkConventionBuilder Instance { get; } = new SharedFrameworkConventionBuilder();

			protected override PartConventionBuilder Configure( ConventionBuilder builder ) => base.Configure( builder ).Shared();
		}

		public class FrameworkConventionBuilder : FactoryBase<ConventionBuilder>
		{
			public static FrameworkConventionBuilder Instance { get; } = new FrameworkConventionBuilder();

			protected override ConventionBuilder CreateItem() => new ConventionBuilder().WithSelf( Configure );

			protected virtual PartConventionBuilder Configure( ConventionBuilder builder ) => builder.ForTypesMatching( type => true ).Export().SelectConstructor( infos => infos.First(), ( info, conventionBuilder ) => conventionBuilder.AsMany( false ) );
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