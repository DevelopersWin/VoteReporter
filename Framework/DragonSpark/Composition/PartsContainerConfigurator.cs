using DragonSpark.Application;
using DragonSpark.Extensions;
using DragonSpark.Sources.Parameterized;
using System;
using System.Collections.Immutable;
using System.Composition.Convention;
using System.Composition.Hosting;

namespace DragonSpark.Composition
{
	public sealed class PartsContainerConfigurator : ContainerConfigurator
	{
		readonly Func<ImmutableArray<Type>> typesSource;
		readonly Func<ConventionBuilder> builderSource;
		readonly Transform<ContainerConfiguration> exportsSource;
		public static PartsContainerConfigurator Default { get; } = new PartsContainerConfigurator();
		PartsContainerConfigurator() : this( ApplicationTypes.Default.Get, ConventionBuilderFactory.Default.Get, ExportedContainerConfigurator.Default.Get ) {}

		public PartsContainerConfigurator( Func<ImmutableArray<Type>> typesSource, Func<ConventionBuilder> builderSource, Transform<ContainerConfiguration> exportsSource )
		{
			this.typesSource = typesSource;
			this.builderSource = builderSource;
			this.exportsSource = exportsSource;
		}

		public override ContainerConfiguration Get( ContainerConfiguration configuration )
		{
			var configured = configuration
				.WithDefaultConventions( builderSource() )
				.WithParts( typesSource().AsEnumerable() )
				.WithProvider( SingletonExportDescriptorProvider.Default )
				.WithProvider( SourceDelegateExporter.Default )
				.WithProvider( ParameterizedSourceDelegateExporter.Default )
				.WithProvider( SourceExporter.Default )
				.WithProvider( TypeInitializingExportDescriptorProvider.Default );
			var result = exportsSource( configured );
			return result;
		}
	}
}