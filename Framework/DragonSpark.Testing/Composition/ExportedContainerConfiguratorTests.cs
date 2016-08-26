using DragonSpark.Composition;
using DragonSpark.Extensions;
using JetBrains.Annotations;
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
		sealed class Configurator : ContainerConfigurator
		{
			[Export, UsedImplicitly]
			public static Configurator Default { get; } = new Configurator();
			Configurator() {}

			public override ContainerConfiguration Get( ContainerConfiguration parameter ) => parameter.WithPart<Additional>();
		}

		interface IAdditional {}

		[Export( typeof(IAdditional) )]
		sealed class Additional : IAdditional {}
	}
}
