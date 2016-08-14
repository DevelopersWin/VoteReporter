using DragonSpark.Activation;
using DragonSpark.Configuration;
using DragonSpark.Extensions;
using DragonSpark.Setup;
using DragonSpark.Sources.Parameterized;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Composition.Convention;
using System.Composition.Hosting;
using System.Composition.Hosting.Core;
using System.Linq;
using System.Reflection;
using CompositeActivator = System.Composition.Hosting.Core.CompositeActivator;

namespace DragonSpark.Composition
{
	public sealed class CompositionHostFactory : ConfigurableFactoryBase<ContainerConfiguration, CompositionHost>
	{
		readonly static IConfigurationScope<ContainerConfiguration> Default = new ConfigurationScope<ContainerConfiguration>( ContainerServicesConfigurator.Instance, PartsContainerConfigurator.Instance );

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

	public class ServicesExportDescriptorProvider : ExportDescriptorProvider
	{
		readonly Func<Type, object> provider;

		public ServicesExportDescriptorProvider() : this( DefaultServiceProvider.Instance ) {}

		public ServicesExportDescriptorProvider( IServiceProvider provider ) : this( new ActivatedServiceSource( provider ).Get ) {}

		ServicesExportDescriptorProvider( Func<Type, object> provider )
		{
			this.provider = provider;
		}

		public override IEnumerable<ExportDescriptorPromise> GetExportDescriptors( CompositionContract contract, DependencyAccessor descriptorAccessor )
		{
			CompositionDependency dependency;
			if ( !descriptorAccessor.TryResolveOptionalDependency( "Existing Request", contract, true, out dependency ) )
			{
				yield return new ExportDescriptorPromise( contract, GetType().FullName, true, NoDependencies, new Factory( provider, contract.ContractType ).Create );
			}
		}

		sealed class Factory : FixedFactory<Type, object>
		{
			readonly CompositeActivator activate;

			public Factory( Func<Type, object> provider, Type contract ) : base( provider, contract )
			{
				activate = Activate;
			}

			object Activate( LifetimeContext context, CompositionOperation operation ) => Get();

			public ExportDescriptor Create( IEnumerable<CompositionDependency> dependencies ) => ExportDescriptor.Create( activate, NoMetadata );
		}
	}

	public class ConventionBuilderFactory : ConfigurableFactoryBase<ConventionBuilder>
	{
		public static ConventionBuilderFactory Instance { get; } = new ConventionBuilderFactory();
		ConventionBuilderFactory() : base( () => new ConventionBuilder(), ConventionTransformer.Instance ) {}
	}

	/*public class ConventionBuilder : System.Composition.Convention.ConventionBuilder
	{
		[Freeze]
		public override IEnumerable<Attribute> GetCustomAttributes( Type reflectedType, MemberInfo member ) => member.GetAttributes<Attribute>();

		[Freeze]
		public override IEnumerable<Attribute> GetCustomAttributes( Type reflectedType, ParameterInfo parameter ) => parameter.GetAttributes<Attribute>();
	}*/

	public class ConventionTransformer : TransformerBase<ConventionBuilder>
	{
		readonly static Func<Type, ConventionMapping> Selector = ConventionMappings.Instance.Get;
		public static ConventionTransformer Instance { get; } = new ConventionTransformer();
		ConventionTransformer() : this( ApplicationTypes.Instance.Get ) {}

		readonly Func<ImmutableArray<Type>> typesSource;

		ConventionTransformer( Func<ImmutableArray<Type>> typesSource )
		{
			this.typesSource = typesSource;
		}

		public override ConventionBuilder Get( ConventionBuilder parameter )
		{
			var mappings = typesSource()
				.Select( Selector )
				.WhereAssigned()
				.ToDictionary( mapping => mapping.InterfaceType, mapping => mapping.ImplementationType );

			foreach ( var mapping in mappings )
			{
				var configure = parameter.ForType( mapping.Value )
										 .Export()
										 .Export( builder => builder.AsContractType( mapping.Key ) );

				var shared = parameter.GetCustomAttributes( mapping.Value, mapping.Value.GetTypeInfo() ).FirstOrDefaultOfType<SharedAttribute>();
				if ( shared != null )
				{
					if ( shared.SharingBoundary != null )
					{
						configure.Shared( shared.SharingBoundary );
					}
					else
					{
						configure.Shared();
					}
				}
			}

			return parameter;
		}
	}

	public sealed class PartsContainerConfigurator : ContainerConfigurator
	{
		readonly Func<ImmutableArray<Type>> typesSource;
		readonly Func<ConventionBuilder> builderSource;
		public static PartsContainerConfigurator Instance { get; } = new PartsContainerConfigurator();
		PartsContainerConfigurator() : this( ApplicationTypes.Instance.Get, ConventionBuilderFactory.Instance.Get ) {}

		public PartsContainerConfigurator( Func<ImmutableArray<Type>> typesSource, Func<ConventionBuilder> builderSource )
		{
			this.typesSource = typesSource;
			this.builderSource = builderSource;
		}

		public override ContainerConfiguration Get( ContainerConfiguration configuration )
		{
			var types = typesSource().ToArray();

			var result = configuration
				.WithParts( types, builderSource() )
				.WithProvider( new SingletonExportDescriptorProvider( types ) )
				.WithProvider( new SourceDelegateExporter() )
				.WithProvider( new ParameterizedSourceDelegateExporter() )
				.WithProvider( new SourceExporter() )
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

		/*class AttributeProvider : AttributedModelProvider
		{
			public static AttributeProvider Instance { get; } = new AttributeProvider();
			AttributeProvider() {}

			[Freeze]
			public override IEnumerable<Attribute> GetCustomAttributes( Type reflectedType, MemberInfo member ) => member.GetAttributes<Attribute>();

			[Freeze]
			public override IEnumerable<Attribute> GetCustomAttributes( Type reflectedType, ParameterInfo parameter ) => parameter.GetAttributes<Attribute>();
		}*/
	}
}