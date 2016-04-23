using System;
using DragonSpark.Activation.IoC;
using DragonSpark.Diagnostics;
using DragonSpark.Extensions;
using DragonSpark.Testing.Framework;
using DragonSpark.Testing.Objects;
using DragonSpark.TypeSystem;
using Microsoft.Practices.Unity;
using Serilog.Events;
using System.Composition;
using System.Linq;
using System.Reflection;
using DragonSpark.Configuration;
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
			Assert.Same( Default<Assembly>.Items, assemblies );

			var levelSwitch = sut.Resolve<IServiceProvider>().Get<LoggingLevelSwitch>();
			Assert.Same( levelSwitch, sut.Resolve<IServiceProvider>().Get<LoggingLevelSwitch>() );
			Configure.Get<DragonSpark.Diagnostics.Configuration>().Profiler.Level = levelSwitch.MinimumLevel = LogEventLevel.Debug;

			var sink = sut.Resolve<IServiceProvider>().Get<LoggerHistorySink>();
			Assert.Same( sink, sut.Resolve<IServiceProvider>().Get<LoggerHistorySink>() );
			var initial = sink.Events.Count();
			Assert.Single( sink.Events );

			var item = sut.TryResolve<IInterface>();
			Assert.Null( item );

			Assert.Same( sut.Resolve<ISingletonLocator>(), sut.Resolve<ISingletonLocator>() );

			Assert.NotEmpty( sink.Events );
			var count = sink.Events.Count();
			Assert.True( count > initial );
		}

		[Export]
		class UnityContainerFactory : Objects.IoC.UnityContainerFactory {}
	}
}