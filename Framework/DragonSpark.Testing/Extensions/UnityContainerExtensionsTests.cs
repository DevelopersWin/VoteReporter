using DragonSpark.Activation;
using DragonSpark.Activation.IoC;
using DragonSpark.Diagnostics;
using DragonSpark.Extensions;
using DragonSpark.Runtime.Properties;
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
	[Trait( Traits.Category, Traits.Categories.ServiceLocation )]
	public class UnityContainerExtensionsTests
	{
		[Theory, AutoData, MinimumLevel( LogEventLevel.Debug )]
		public void TryResolve( [Factory]UnityContainer sut )
		{
			var creator = sut.Get( Creator.Default );
			Assert.IsType<UnityContainerCoreFactory>( creator );

			var provider = sut.Resolve<IServiceProvider>();
			var sink = provider.Get<LoggerHistorySink>();
			
			var initial = sink.Events.Count();
			Assert.Single( sink.Events );

			var item = sut.TryResolve<IInterface>();
			Assert.Null( item );

			Assert.NotEmpty( sink.Events );
			var count = sink.Events.Count();
			Assert.True( count > initial );

			/*Assert.Equal( TryContextProperty.Debug.Get( sut ), TryContextProperty.Debug.Get( sut ) );
			Assert.NotEqual( TryContextProperty.Debug.Get( sut ), TryContextProperty.Verbose.Get( sut ) );*/

			Assert.Same( provider.Get<LoggingLevelSwitch>(), provider.Get<LoggingLevelSwitch>() );
			Assert.Same( sut.Resolve<ISingletonLocator>(), sut.Resolve<ISingletonLocator>() );
			Assert.Same( sink, provider.Get<LoggerHistorySink>() );
			Assert.Same( Items<Assembly>.Default, sut.Resolve<Assembly[]>() );
		}

		[Export]
		class UnityContainerFactory : Objects.IoC.UnityContainerFactory {}
	}
}