using System;
using DragonSpark.Application;
using DragonSpark.Diagnostics.Logging;
using DragonSpark.Setup;
using DragonSpark.Sources;

namespace DragonSpark.Activation.Location
{
	public sealed class DefaultServiceProvider : CompositeServiceProvider
	{
		public static IServiceProvider Default { get; } = new DefaultServiceProvider();
		DefaultServiceProvider() : base( new SourceServiceProvider( GlobalServiceProvider.Default, Activator.Default, Exports.Default, ApplicationParts.Default, ApplicationAssemblies.Default, ApplicationTypes.Default, LoggingHistory.Default, LoggingController.Default, Logger.Default.ToScope(), Instances.Default ), new DecoratedServiceProvider( Instances.Get<object> ), new DecoratedServiceProvider( Activator.Activate<object> ) ) {}
	}
}