using System.Composition.Hosting;
using DragonSpark.Configuration;

namespace DragonSpark.Composition
{
	public sealed class CompositionHostFactory : ConfigurableFactoryBase<ContainerConfiguration, CompositionHost>
	{
		readonly static IConfigurationScope<ContainerConfiguration> DefaultConfiguration = new ConfigurationScope<ContainerConfiguration>( ContainerServicesConfigurator.Default, PartsContainerConfigurator.Default );

		public static CompositionHostFactory Default { get; } = new CompositionHostFactory();
		CompositionHostFactory() : base( () => new ContainerConfiguration(), DefaultConfiguration, parameter => parameter.CreateContainer() ) {}
	}
}