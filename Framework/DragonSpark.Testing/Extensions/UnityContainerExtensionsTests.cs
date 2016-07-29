using DragonSpark.Activation;
using DragonSpark.Diagnostics;
using DragonSpark.Extensions;
using DragonSpark.Testing.Framework;
using DragonSpark.Testing.Framework.Setup;
using DragonSpark.Testing.Objects;
using DragonSpark.TypeSystem;
using Microsoft.Practices.Unity;
using Serilog.Core;
using Serilog.Events;
using System;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Reflection;
using Xunit;

namespace DragonSpark.Testing.Extensions
{
	[Trait( Traits.Category, Traits.Categories.ServiceLocation ), ContainingTypeAndNested]
	public class UnityContainerExtensionsTests
	{
		[Export]
		public IUnityContainer Container { get; } = DragonSpark.Activation.IoC.UnityContainerFactory.Instance.Create();

		[Theory, AutoData, MinimumLevel( LogEventLevel.Debug )]
		public void TryResolve( UnityContainer sut )
		{
			var creator = Creator.Default.Get( sut );
			Assert.IsType<DragonSpark.Activation.IoC.UnityContainerFactory>( creator );

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
			Assert.Equal( Items<Assembly>.Immutable, sut.Resolve<ImmutableArray<Assembly>>() );
		}

		/*[Export]
		class UnityContainerFactory : Objects.IoC.UnityContainerFactory {}*/
	}
}