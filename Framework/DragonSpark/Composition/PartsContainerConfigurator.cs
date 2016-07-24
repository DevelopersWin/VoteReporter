using DragonSpark.Activation;
using DragonSpark.Aspects;
using DragonSpark.Configuration;
using DragonSpark.Extensions;
using DragonSpark.Runtime.Stores;
using DragonSpark.Setup;
using System;
using System.Collections.Generic;
using System.Composition.Convention;
using System.Composition.Hosting;
using System.Composition.Hosting.Core;
using System.Linq;
using System.Reflection;
using CompositeActivator = System.Composition.Hosting.Core.CompositeActivator;

namespace DragonSpark.Composition
{
	public sealed class CompositionHostFactory : AggregateFactoryBase<ContainerConfiguration, CompositionHost>
	{
		readonly static IConfigurations<ContainerConfiguration> Default = new Configurations<ContainerConfiguration>( ContainerServicesConfigurator.Instance, PartsContainerConfigurator.Instance );

		public static CompositionHostFactory Instance { get; } = new CompositionHostFactory();
		CompositionHostFactory() : base( () => new ContainerConfiguration(), Default, parameter => parameter.CreateContainer()) {}
	}

	public abstract class ContainerConfigurator : TransformerBase<ContainerConfiguration> {}

	public class ContainerServicesConfigurator : ContainerConfigurator
	{
		public static ContainerServicesConfigurator Instance { get; } = new ContainerServicesConfigurator();
		ContainerServicesConfigurator() {}

		public override ContainerConfiguration Get( ContainerConfiguration parameter ) => parameter.WithProvider( new ServicesExportDescriptorProvider() );
	}

	public class ServicesExportDescriptorProvider : ExportDescriptorProvider, IDependencyLocatorKey
	{
		readonly Func<IDependencyLocatorKey, ServiceSource> locator;
		readonly InstanceExportDescriptorProvider<IDependencyLocatorKey> key;
		readonly Func<Type, object> get;

		public ServicesExportDescriptorProvider() : this( DependencyLocator.Instance.For ) {}

		protected ServicesExportDescriptorProvider( Func<IDependencyLocatorKey, ServiceSource> locator )
		{
			this.locator = locator;
			key = new InstanceExportDescriptorProvider<IDependencyLocatorKey>( this );
			get = Get;
		}

		public override IEnumerable<ExportDescriptorPromise> GetExportDescriptors( CompositionContract contract, DependencyAccessor descriptorAccessor )
		{
			var instance = key.GetExportDescriptors( contract, descriptorAccessor ).Fixed();
			var result = instance.Any() ? instance : GetDependency( contract, descriptorAccessor );
			return result;
		}

		IEnumerable<ExportDescriptorPromise> GetDependency( CompositionContract contract, DependencyAccessor descriptorAccessor )
		{
			CompositionDependency dependency;
			if ( !descriptorAccessor.TryResolveOptionalDependency( "Existing Request", contract, true, out dependency ) )
			{
				yield return new ExportDescriptorPromise( contract, GetType().FullName, true, NoDependencies, new Factory( get, contract.ContractType ).Create );
			}
		}

		object Get( Type type )
		{
			var invoke = locator( this )?.Invoke( type );
			return invoke;
		}

		sealed class Factory : FixedFactory<Type, object>
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

	public sealed class PartsContainerConfigurator : ContainerConfigurator
	{
		// readonly static ConventionBuilder ConventionBuilder = FrameworkConventionBuilder.Instance.Create();

		public static PartsContainerConfigurator Instance { get; } = new PartsContainerConfigurator();
		PartsContainerConfigurator() {}

		public override ContainerConfiguration Get( ContainerConfiguration configuration )
		{
			var system = ApplicationParts.Instance.Get();
			// var core = FrameworkTypes.Instance.Get().ToArray();
			var types = system.Types.ToArray()/*.ToArray().Union( core ).ToImmutableArray()*/;

			var result = configuration
				.WithInstance( system )
				.WithInstance( system.Assemblies )
				.WithInstance( system.Assemblies.ToArray() )
				.WithInstance( system.Types )
				.WithInstance( types )
				.WithInstance( Activation.Activator.Instance.Get() )
				// .WithParts( core, ConventionBuilder )
				.WithParts( types, AttributeProvider.Instance )
				.WithProvider( new FactoryDelegateExportDescriptorProvider() )
				.WithProvider( new FactoryWithParameterDelegateExportDescriptorProvider() )
				.WithProvider( new FactoryExportDescriptorProvider() )
				.WithProvider( new TypeInitializingExportDescriptorProvider() );
			return result;
		}

		/*class FrameworkConventionBuilder : FactoryBase<ConventionBuilder>
		{
			readonly Func<ConventionBuilder, object> configure;

			public static FrameworkConventionBuilder Instance { get; } = new FrameworkConventionBuilder();
			FrameworkConventionBuilder()
			{
				configure = Configure;
			}

			public override ConventionBuilder Create() => new ConventionBuilder().WithSelf( configure );

			static PartConventionBuilder Configure( ConventionBuilder builder ) => builder.ForTypesMatching( type => true ).Export().SelectConstructor( infos => infos.First(), ( info, conventionBuilder ) => conventionBuilder.AsMany( false ) );
		}*/

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