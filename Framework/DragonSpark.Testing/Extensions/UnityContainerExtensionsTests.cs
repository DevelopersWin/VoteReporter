using DragonSpark.Activation.IoC;
using DragonSpark.Diagnostics;
using DragonSpark.Extensions;
using DragonSpark.Testing.Framework;
using DragonSpark.Testing.Framework.Setup;
using DragonSpark.Testing.Objects;
using DragonSpark.TypeSystem;
using Microsoft.Practices.Unity;
using Serilog.Events;
using System;
using System.Composition;
using System.Linq;
using System.Reflection;
using Xunit;
using LoggingLevelSwitch = Serilog.Core.LoggingLevelSwitch;

namespace DragonSpark.Testing.Extensions
{
	public class UnityContainerExtensionsTests
	{
		[Theory, AutoData, MinimumLevel( LogEventLevel.Debug )]
		public void TryResolve( [Factory]UnityContainer sut )
		{
			var assemblies = sut.Resolve<Assembly[]>();
			Assert.Same( Default<Assembly>.Items, assemblies );

			var provider = sut.Resolve<IServiceProvider>();
			var levelSwitch = provider.Get<LoggingLevelSwitch>();
			Assert.Same( levelSwitch, provider.Get<LoggingLevelSwitch>() );
			

			var sink = provider.Get<LoggerHistorySink>();
			Assert.Same( sink, provider.Get<LoggerHistorySink>() );
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