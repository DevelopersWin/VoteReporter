using DragonSpark.Activation;
using DragonSpark.Activation.IoC;
using DragonSpark.Diagnostics;
using DragonSpark.Extensions;
using DragonSpark.Runtime.Values;
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
			var creator = sut.Get( Creator.Property );
			Assert.IsType<UnityContainerFactory>( creator );

			var provider = sut.Resolve<IServiceProvider>();
			var sink = provider.Get<LoggerHistorySink>();
			
			var initial = sink.Events.Count();
			Assert.Single( sink.Events );

			var item = sut.TryResolve<IInterface>();
			Assert.Null( item );

			Assert.NotEmpty( sink.Events );
			var count = sink.Events.Count();
			Assert.True( count > initial );

			Assert.Same( sut.Resolve<TryContextElevated>(), sut.Resolve<TryContextElevated>() );

			Assert.Same( provider.Get<LoggingLevelSwitch>(), provider.Get<LoggingLevelSwitch>() );
			Assert.Same( sut.Resolve<ISingletonLocator>(), sut.Resolve<ISingletonLocator>() );
			Assert.Same( sink, provider.Get<LoggerHistorySink>() );
			Assert.Same( Default<Assembly>.Items, sut.Resolve<Assembly[]>() );
		}

		[Export]
		class UnityContainerFactory : Objects.IoC.UnityContainerFactory {}
	}
}