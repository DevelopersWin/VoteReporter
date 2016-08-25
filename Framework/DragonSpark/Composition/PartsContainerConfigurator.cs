using DragonSpark.Activation.Location;
using DragonSpark.Application;
using DragonSpark.Application.Setup;
using DragonSpark.Configuration;
using DragonSpark.Extensions;
using DragonSpark.Sources.Parameterized;
using DragonSpark.Specifications;
using DragonSpark.TypeSystem.Metadata;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Composition.Convention;
using System.Composition.Hosting;
using System.Composition.Hosting.Core;
using System.Linq;
using CompositeActivator = System.Composition.Hosting.Core.CompositeActivator;

namespace DragonSpark.Composition
{
	public sealed class ContainerServicesConfigurator : ContainerConfigurator
	{
		public static ContainerServicesConfigurator Default { get; } = new ContainerServicesConfigurator();
		ContainerServicesConfigurator() {}

		public override ContainerConfiguration Get( ContainerConfiguration parameter ) => parameter.WithProvider( ServicesExportDescriptorProvider.Default );
	}

	public sealed class ServicesExportDescriptorProvider : ExportDescriptorProvider
	{
		public static ServicesExportDescriptorProvider Default { get; } = new ServicesExportDescriptorProvider();
		ServicesExportDescriptorProvider() : this( DefaultServiceProvider.Default ) {}

		readonly Func<Type, object> provider;

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
		public static ConventionBuilderFactory Default { get; } = new ConventionBuilderFactory();
		ConventionBuilderFactory() : base( () => new ConventionBuilder(), ConventionTransformer.Default ) {}
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
		readonly static Func<Type, ConventionMapping> Selector = ConventionMappings.Default.Get;

		public static ConventionTransformer Default { get; } = new ConventionTransformer();
		ConventionTransformer() : this( ApplicationTypes.Default.ToDelegate(), Defaults.IsExportSpecification.ToSpecificationDelegate() ) {}

		readonly Func<ImmutableArray<Type>> typesSource;
		readonly Func<Type, bool> containsExports;

		ConventionTransformer( Func<ImmutableArray<Type>> typesSource, Func<Type, bool> containsExports )
		{
			this.typesSource = typesSource;
			this.containsExports = containsExports;
		}

		public override ConventionBuilder Get( ConventionBuilder parameter )
		{
			var mappings = typesSource()
				.Select( Selector )
				.WhereAssigned()
				.Distinct( mapping => mapping.InterfaceType )
				;

			foreach ( var mapping in mappings.ToArray() )
			{
				if ( !containsExports( mapping.ImplementationType ) )
				{
					var configure = parameter.ForType( mapping.ImplementationType )
										 .Export()
										 .Export( builder => builder.AsContractType( mapping.InterfaceType ) );

					var shared = AttributeSupport<SharedAttribute>.Local.Get( mapping.ImplementationType );
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
			}

			return parameter;
		}
	}

	public sealed class PartsContainerConfigurator : ContainerConfigurator
	{
		readonly Func<ImmutableArray<Type>> typesSource;
		readonly Func<ConventionBuilder> builderSource;
		public static PartsContainerConfigurator Default { get; } = new PartsContainerConfigurator();
		PartsContainerConfigurator() : this( ApplicationTypes.Default.Get, ConventionBuilderFactory.Default.Get ) {}

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
				.WithProvider( SourceDelegateExporter.Default )
				.WithProvider( ParameterizedSourceDelegateExporter.Default )
				.WithProvider( SourceExporter.Default )
				.WithProvider( TypeInitializingExportDescriptorProvider.Default );
			return result;
		}
	}
}