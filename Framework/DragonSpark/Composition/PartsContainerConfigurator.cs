using DragonSpark.Application;
using DragonSpark.Extensions;
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
		public static PartsContainerConfigurator Default { get; } = new PartsContainerConfigurator();
		PartsContainerConfigurator() : this( ApplicationTypes.Default.Get, ConventionBuilderFactory.Default.Get ) {}

		public PartsContainerConfigurator( Func<ImmutableArray<Type>> typesSource, Func<ConventionBuilder> builderSource )
		{
			this.typesSource = typesSource;
			this.builderSource = builderSource;
		}

		public override ContainerConfiguration Get( ContainerConfiguration configuration )
		{
			var result = configuration
				.WithDefaultConventions( builderSource() )
				.WithParts( typesSource().AsEnumerable() )
				.WithProvider( SingletonExportDescriptorProvider.Default )
				.WithProvider( SourceDelegateExporter.Default )
				.WithProvider( ParameterizedSourceDelegateExporter.Default )
				.WithProvider( SourceExporter.Default )
				.WithProvider( TypeInitializingExportDescriptorProvider.Default );
			return result;
		}
	}

	public class ExportedContainerConfigurator : ContainerConfigurator
	{
		public override ContainerConfiguration Get( ContainerConfiguration parameter )
		{
			return null;
		}
	}
}