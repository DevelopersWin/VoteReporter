using DragonSpark.Testing.Framework;
using DragonSpark.Testing.Framework.Setup;
using Xunit;

namespace DragonSpark.Testing.Extensions
{
	[Trait( Traits.Category, Traits.Categories.ServiceLocation ), FrameworkTypes]
	public class UnityContainerExtensionsTests
	{
		/*[Theory, AutoData, MinimumLevel( LogEventLevel.Debug )]
		public void TryResolve( IUnityContainer sut )
		{
			var level = MinimumLevelConfiguration.Default.Get();
			Assert.Equal( LogEventLevel.Debug, level );

			var creator = Origin.Default.Get( sut );
			Assert.IsType<UnityContainerFactory>( creator );

			var provider = sut.Resolve<IServiceProvider>();
			Assert.NotNull( provider );
			Assert.Same( GlobalServiceProvider.Default.Get(), provider );
			Assert.Same( GlobalServiceProvider.Default.Get(), DefaultServiceProvider.Default.Get<IServiceProvider>() );

			var sink = LoggingHistory.Default.Get( sut );
			
			var initial = sink.Events.Count();
			Assert.NotEmpty( sink.Events );

			var item = sut.TryResolve<IInterface>();
			Assert.Null( item );

			Assert.NotEmpty( sink.Events );
			var count = sink.Events.Count();
			Assert.True( count > initial );

			var typeDefinitionProviders = DragonSpark.TypeSystem.Configuration.TypeDefinitionProviders.Get();
			Assert.Contains( MetadataTypeDefinitionProvider.Default, typeDefinitionProviders );

			/*var levelSwitch = provider.Get<LoggingLevelSwitch>();
			Assert.Same( levelSwitch, provider.Get<LoggingLevelSwitch>() );#1#
			Assert.Same( sut.Resolve<ISingletonLocator>(), sut.Resolve<ISingletonLocator>() );
			Assert.Same( SingletonLocator.Default, sut.Resolve<ISingletonLocator>() );
			// Assert.Same( sink, provider.Get<LoggerHistorySink>() );
			Assert.Equal( ApplicationAssemblies.Default.Get(), sut.Resolve<ImmutableArray<Assembly>>() );
		}*/

		/*[Export]
		class UnityContainerFactory : Objects.IoC.UnityContainerFactory {}*/
	}
}