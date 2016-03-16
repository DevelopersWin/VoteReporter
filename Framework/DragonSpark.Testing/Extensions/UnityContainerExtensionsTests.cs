using DragonSpark.Activation.FactoryModel;
using DragonSpark.Activation.IoC;
using DragonSpark.Diagnostics;
using DragonSpark.Extensions;
using DragonSpark.Testing.Framework;
using DragonSpark.Testing.Objects;
using Microsoft.Practices.Unity;
using Serilog.Events;
using System.Composition;
using System.Linq;
using Xunit;
using LoggingLevelSwitch = Serilog.Core.LoggingLevelSwitch;

namespace DragonSpark.Testing.Extensions
{
	public class UnityContainerExtensionsTests
	{
		[Theory, Framework.Setup.AutoData]
		public void TryResolve( [Factory]UnityContainer sut )
		{
			var levelSwitch = sut.Resolve<LoggingLevelSwitch>();
			levelSwitch.MinimumLevel = LogEventLevel.Debug;

			var logger = sut.Resolve<RecordingLogEventSink>();
			Assert.Same( logger, sut.Resolve<RecordingLogEventSink>() );
			var initial = logger.Events.Count();
			Assert.NotEmpty( logger.Events );

			// Assert.False( sut.IsRegistered<ISingletonLocator>() );

			var item = sut.TryResolve<IInterface>();
			Assert.Null( item );

			Assert.Same( sut.Resolve<ISingletonLocator>(), sut.Resolve<ISingletonLocator>() );

			// Assert.True( sut.IsRegistered<ISingletonLocator>() );

			var count = logger.Events.Count();
			Assert.True( count > initial );
		}

		[Export]
		class UnityContainerFactory : Objects.Setup.UnityContainerFactory {}
	}
}