using DragonSpark.Activation;
using DragonSpark.Activation.IoC;
using DragonSpark.Diagnostics;
using DragonSpark.Diagnostics.Logger;
using DragonSpark.Extensions;
using DragonSpark.Setup;
using DragonSpark.Testing.Framework;
using DragonSpark.Testing.Framework.IoC;
using DragonSpark.Testing.Framework.Parameters;
using DragonSpark.Testing.Framework.Setup;
using DragonSpark.Testing.Objects;
using DragonSpark.TypeSystem;
using DragonSpark.Windows.Runtime;
using Microsoft.Practices.Unity;
using Serilog.Events;
using System;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using Xunit;

namespace DragonSpark.Testing.Extensions
{
	[Trait( Traits.Category, Traits.Categories.ServiceLocation ), FrameworkTypes, IoCTypes]
	public class UnityContainerExtensionsTests
	{
		[Theory, Framework.Setup.AutoData, MinimumLevel( LogEventLevel.Debug )]
		public void TryResolve( [Service]IUnityContainer sut )
		{
			var level = MinimumLevelConfiguration.Instance.Get( this );
			Assert.Equal( LogEventLevel.Debug, level );

			var creator = Creator.Default.Get( sut );
			Assert.IsType<UnityContainerFactory>( creator );

			var provider = sut.Resolve<IServiceProvider>();
			Assert.NotNull( provider );
			Assert.Same( GlobalServiceProvider.Instance.Get(), provider );
			Assert.Same( GlobalServiceProvider.Instance.Get(), DefaultServiceProvider.Instance.Get<IServiceProvider>() );

			var sink = LoggingHistory.Instance.Get( sut );
			
			var initial = sink.Events.Count();
			Assert.NotEmpty( sink.Events );

			var item = sut.TryResolve<IInterface>();
			Assert.Null( item );

			Assert.NotEmpty( sink.Events );
			var count = sink.Events.Count();
			Assert.True( count > initial );

			Assert.Contains( MetadataTypeDefinitionProvider.Instance, AttributeConfigurations.TypeDefinitionProviders.Get() );

			/*var levelSwitch = provider.Get<LoggingLevelSwitch>();
			Assert.Same( levelSwitch, provider.Get<LoggingLevelSwitch>() );*/
			Assert.Same( sut.Resolve<ISingletonLocator>(), sut.Resolve<ISingletonLocator>() );
			Assert.Same( SingletonLocator.Instance, sut.Resolve<ISingletonLocator>() );
			// Assert.Same( sink, provider.Get<LoggerHistorySink>() );
			Assert.Equal( ApplicationAssemblies.Instance.Get(), sut.Resolve<ImmutableArray<Assembly>>() );
		}

		/*[Export]
		class UnityContainerFactory : Objects.IoC.UnityContainerFactory {}*/
	}
}