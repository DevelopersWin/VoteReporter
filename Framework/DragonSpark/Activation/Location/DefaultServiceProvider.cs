using DragonSpark.Application;
using DragonSpark.Application.Setup;
using DragonSpark.Diagnostics.Logging;
using DragonSpark.Sources;

namespace DragonSpark.Activation.Location
{
	public sealed class DefaultServiceProvider : CompositeServiceProvider
	{
		public static DefaultServiceProvider Default { get; } = new DefaultServiceProvider();
		DefaultServiceProvider() : base( new InstanceRepository( GlobalServiceProvider.Default, Activator.Default, Exports.Default, ApplicationParts.Default, ApplicationAssemblies.Default, ApplicationTypes.Default, LoggingHistory.Default, LoggingController.Default, Logger.Default.ToScope(), Instances.Default ), new DecoratedServiceProvider( Instances.Default.Get ), new DecoratedServiceProvider( Activator.Default.Get ) ) {}
	}
}