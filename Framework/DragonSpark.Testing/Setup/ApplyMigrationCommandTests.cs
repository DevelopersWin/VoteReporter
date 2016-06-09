using DragonSpark.Activation;
using DragonSpark.Diagnostics;
using DragonSpark.Extensions;
using DragonSpark.Runtime;
using DragonSpark.Runtime.Properties;
using DragonSpark.Setup;
using DragonSpark.Testing.Framework;
using Ploeh.AutoFixture.Xunit2;
using Serilog;
using Serilog.Core;
using System;
using System.Linq;
using System.Reflection;
using Xunit;

namespace DragonSpark.Testing.Setup
{
	public class ApplyMigrationCommandTests
	{
		[Theory, AutoData]
		public void EnsureMigrationPerformsAsExpected( RecordingLoggerFactory source, ServiceProvider destination, string message )
		{
			using ( MethodBase.GetCurrentMethod().AsCurrentContext( source ) )
			{
				var current = DefaultServiceProvider.Instance.Value;
				Assert.Same( source.History, current.Get<ILoggerHistory>() );
				Assert.Same( source.LevelSwitch, current.Get<LoggingLevelSwitch>() );

				Assert.Empty( source.History.Events );

				var logger = current.Get<ILogger>();
				logger.Information( $"This is from the source logger: {message}" );

				Assert.Single( source.History.Events );

				var only = source.History.Events.Only();
				var text = LogEventTextFactory.Instance.Create( only );
				Assert.Contains( message, text );

				var destinationHistory = destination.Get<ILoggerHistory>();
				Assert.Empty( destinationHistory.Events );
				ApplyMigrationCommand.Instance.Execute( new MigrationParameter<IServiceProvider>( current, destination ) );

				Assert.Empty( source.History.Events );

				Assert.Equal( 2, destinationHistory.Events.Count() );
				Assert.Contains( only, destinationHistory.Events );
			}
		}

		[Theory, AutoData]
		public void EnsureMigrationSourceUtilizedAsExpected( RecordingLoggerFactory factory, string message )
		{
			using ( MethodBase.GetCurrentMethod().AsCurrentContext( factory ) )
			{
				var current = DefaultServiceProvider.Instance.Value;
				
				var logger = current.Get<ILogger>();
				logger.Information( $"This is from the source logger: {message}" );
				Assert.Single( factory.History.Events );

				var only = factory.History.Events.Only();
				var text = LogEventTextFactory.Instance.Create( only );
				Assert.Contains( message, text );

				var source = new Source();
				var destination = new CompositeServiceProvider( new InstanceServiceProvider( source ), new ServiceProvider() );

				Assert.NotNull( destination.Get<IServiceProviderMigrationCommandSource>() );

				var destinationHistory = destination.Get<ILoggerHistory>();
				Assert.False( factory.History.Get( Source.Property ).IsApplied );
				Assert.Empty( destinationHistory.Events );
				ApplyMigrationCommand.Instance.Execute( new MigrationParameter<IServiceProvider>( current, destination ) );
				Assert.True( factory.History.Get( Source.Property ).IsApplied );
			}
		}

		class Source : ServiceProviderMigrationCommandFactory
		{
			public static Condition Property { get; } = new Condition();

			public override ICommand<MigrationParameter<IServiceProvider>> Create( IServiceProvider parameter )
			{
				parameter.Get<ILoggerHistory>().Get( Property ).Apply();
				return base.Create( parameter );
			}
		}
	}
}