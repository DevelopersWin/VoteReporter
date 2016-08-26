using System;
using System.Collections.Immutable;
using System.Composition.Hosting;
using System.Linq;
using DragonSpark.Sources;

namespace DragonSpark.Composition
{
	public sealed class ExportedContainerConfigurator : ContainerConfigurator
	{
		public static ExportedContainerConfigurator Default { get; } = new ExportedContainerConfigurator();
		ExportedContainerConfigurator() : this( ExportSource<ContainerConfigurator>.Default.Get ) {}

		readonly Func<ImmutableArray<ContainerConfigurator>> source;

		public ExportedContainerConfigurator( Func<ImmutableArray<ContainerConfigurator>> source )
		{
			this.source = source;
		}

		public override ContainerConfiguration Get( ContainerConfiguration parameter ) => 
			source().Aggregate( parameter, ( current, configurator ) => configurator.Get( current ) );
	}
}