using DragonSpark.Activation.FactoryModel;
using DragonSpark.Activation.IoC;
using DragonSpark.Composition;
using DragonSpark.Diagnostics;
using DragonSpark.Extensions;
using DragonSpark.Testing.Framework;
using DragonSpark.Testing.Objects;
using Microsoft.Practices.Unity;
using Serilog.Events;
using System.Composition;
using System.Linq;
using System.Reflection;
using DragonSpark.TypeSystem;
using Serilog;
using Xunit;
using LoggingLevelSwitch = Serilog.Core.LoggingLevelSwitch;

namespace DragonSpark.Testing.Extensions
{
	public class UnityContainerExtensionsTests
	{
		[Theory, Framework.Setup.AutoData]
		public void TryResolve( [Factory]UnityContainer sut )
		{
			var assemblies = sut.Resolve<Assembly[]>();
			Assert.Equal( Default<Assembly>.Items, assemblies);

			var levelSwitch = sut.Resolve<LoggingLevelSwitch>();
			Assert.Same( levelSwitch, sut.Resolve<LoggingLevelSwitch>() );
			levelSwitch.MinimumLevel = LogEventLevel.Debug;

			var sink = sut.Resolve<RecordingLogEventSink>();
			Assert.Same( sink, sut.Resolve<RecordingLogEventSink>() );
			var initial = sink.Events.Count();
			Assert.NotEmpty( sink.Events );

			var item = sut.TryResolve<IInterface>();
			Assert.Null( item );

			Assert.Same( sut.Resolve<ISingletonLocator>(), sut.Resolve<ISingletonLocator>() );

			// Assert.True( sut.IsRegistered<ISingletonLocator>() );

			Assert.NotEmpty( sink.Events );
			var count = sink.Events.Count();
			Assert.True( count > initial );
		}

		[Export]
		class UnityContainerFactory : Objects.Setup.UnityContainerFactory {}
	}
}