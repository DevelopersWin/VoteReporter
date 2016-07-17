using DragonSpark.Activation;
using DragonSpark.Aspects;
using DragonSpark.Configuration;
using DragonSpark.Extensions;
using DragonSpark.Runtime.Stores;
using DragonSpark.Setup;
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
	public sealed class CompositionSource : Configuration<CompositionHost>
	{
		public static CompositionSource Instance { get; } = new CompositionSource();
		CompositionSource() : base( () => ConfigurationSource.Instance.Get().CreateContainer() ) {}
	}

	public sealed class ConfigurationSource : Configuration<ContainerConfiguration>
	{
		public static ConfigurationSource Instance { get; } = new ConfigurationSource();
		ConfigurationSource() : base( () => Configurations.Instance.Get().Aggregate( new ContainerConfiguration(), ( configuration, transformer ) => transformer.Create( configuration ) ) ) {}
	}

	public sealed class Configurations : Configuration<ImmutableArray<ContainerConfigurator>>
	{
		public static Configurations Instance { get; } = new Configurations();
		Configurations() : base( () => ImmutableArray.Create<ContainerConfigurator>( ContainerServicesConfigurator.Instance, new PartsContainerConfigurator( DefaultTypeSystem.Instance.Get() ) ) ) {}
	}

	/*public class AssemblyBasedConfigurationContainerFactory : ContainerConfigurationFromPartsFactory
	{
		public AssemblyBasedConfigurationContainerFactory( IEnumerable<Assembly> assemblies ) : this( assemblies.ToImmutableArray() ) { }

		AssemblyBasedConfigurationContainerFactory( ImmutableArray<Assembly> assemblies )
			: base( assemblies, TypesFactory.Instance.Create( assemblies ) ) {}
	}

	public class TypeBasedConfigurationContainerFactory : ContainerConfigurationFromPartsFactory
	{
		public TypeBasedConfigurationContainerFactory( IEnumerable<Type> types )  : base( Items<Assembly>.Immutable, types.ToImmutableArray() ) {}
	}

	public class ContainerConfigurationFromPartsFactory : AggregateFactory<ContainerConfiguration>
	{
		public ContainerConfigurationFromPartsFactory( ImmutableArray<Assembly> assemblies, ImmutableArray<Type> types )
			: base( ContainerConfigurationFactory.Instance.Create, ContainerServicesConfigurator.Instance.ToDelegate(), new PartsContainerConfigurator( assemblies, types ).ToDelegate() ) {}
	}*/

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
			Assign( DefaultServiceProvider.Instance );
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
		readonly static ConventionBuilder ConventionBuilder = FrameworkConventionBuilder.Instance.Create();
		readonly Setup.TypeSystem system;
		readonly ImmutableArray<Type> core, all;

		public PartsContainerConfigurator( Setup.TypeSystem system ) : this( system, FrameworkTypes.Instance.Get(), AllTypes.Instance.Get() ) {}

		PartsContainerConfigurator( Setup.TypeSystem system, ImmutableArray<Type> core, ImmutableArray<Type> all )
		{
			this.system = system;
			this.core = core;
			this.all = all;
		}

		public override ContainerConfiguration Create( ContainerConfiguration configuration )
		{
			var result = configuration
				.WithInstance( system.Assemblies )
				.WithInstance( system.Assemblies.ToArray() )
				.WithInstance( all )
				.WithInstance( all.ToArray() )
				.WithInstance( Activation.Activator.Instance.Get() )
				.WithParts( core.ToArray(), ConventionBuilder )
				.WithParts( system.Types.ToArray(), AttributeProvider.Instance )
				.WithProvider( new FactoryDelegateExportDescriptorProvider() )
				.WithProvider( new FactoryWithParameterDelegateExportDescriptorProvider() )
				.WithProvider( new FactoryExportDescriptorProvider() )
				.WithProvider( new TypeInitializingExportDescriptorProvider() );
			return result;
		}

		public class FrameworkConventionBuilder : FactoryBase<ConventionBuilder>
		{
			readonly Func<ConventionBuilder, object> configure;

			public static FrameworkConventionBuilder Instance { get; } = new FrameworkConventionBuilder();
			FrameworkConventionBuilder()
			{
				configure = Configure;
			}

			public override ConventionBuilder Create() => new ConventionBuilder().WithSelf( configure );

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