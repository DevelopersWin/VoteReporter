using DragonSpark.Composition;
using DragonSpark.Extensions;
using DragonSpark.Sources;
using JetBrains.Annotations;
using System.Collections.Generic;
using System.Composition;
using System.Composition.Hosting;
using Xunit;

namespace DragonSpark.Testing.Composition
{
	public class ExportedContainerConfiguratorTests
	{
		[Fact]
		public void Verify()
		{
			GetType().Adapt().WithNested().AsApplicationParts();
			var container = ExportedContainerConfigurator.Default.Get( new ContainerConfiguration() ).CreateContainer();
			var export = container.GetExport<IAdditional>();
			Assert.IsType<Additional>( export );
		}

		[UsedImplicitly]
		sealed class Configurator : ContainerConfiguratorMappings
		{
			[Export, UsedImplicitly]
			public static Configurator Default { get; } = new Configurator();
			Configurator() : base( Mappings.Default ) {}
		}

		class Mappings : ItemSourceBase<ExportMapping>
		{
			public static Mappings Default { get; } = new Mappings();
			Mappings() {}

			protected override IEnumerable<ExportMapping> Yield()
			{
				yield return new ExportMapping<Additional, IAdditional>();
			}
		}

		interface IAdditional {}

		[Export( typeof(IAdditional) )]
		sealed class Additional : IAdditional {}
	}
}
