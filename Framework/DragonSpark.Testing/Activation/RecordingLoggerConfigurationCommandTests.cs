using DragonSpark.Activation;
using DragonSpark.Configuration;
using DragonSpark.Diagnostics;
using Ploeh.AutoFixture.Xunit2;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace DragonSpark.Testing.Activation
{
	public class RecordingLoggerConfigurationCommandTests
	{
		[Fact]
		public void BasicContext()
		{
			ConfigureLoggerConfigurationsCommand.Instance.Execute( RecordingLoggerConfigurationsFactory.Instance.Create );

			var level = MinimumLevelConfiguration.Instance;
			var controller = LoggingLevelSwitchConfiguration.Instance;

			var one = new object();
			var first = controller.Get( one );
			Assert.Same( first, controller.Get( one ) );

			Assert.Equal( LogEventLevel.Information, first.MinimumLevel );

			const LogEventLevel assigned = LogEventLevel.Debug;
			level.Assign( o => assigned );

			var two = new object();
			var second = controller.Get( two );
			Assert.NotSame( first, second );
			Assert.Same( second, controller.Get( two ) );

			Assert.Equal( assigned, second.MinimumLevel );
		}

		[Theory, AutoData]
		void VerifyHistory( object context, string message )
		{
			ConfigureLoggerConfigurationsCommand.Instance.Execute( RecordingLoggerConfigurationsFactory.Instance.Create );

			var history = LoggerHistoryConfiguration.Instance.Get( context );
			Assert.Empty( history.Events );
			Assert.Same( history, LoggerHistoryConfiguration.Instance.Get( context ) );

			var logger = LoggerCongfiguration.Instance.Get( context );
			Assert.Empty( history.Events );
			logger.Information( "Hello World! {Message}", message );
			Assert.Single( history.Events, item => item.RenderMessage().Contains( message ) );

			logger.Debug( "Hello World! {Message}", message );
			Assert.Single( history.Events );
			var level = LoggingLevelSwitchConfiguration.Instance.Get( context );
			level.MinimumLevel = LogEventLevel.Debug;

			logger.Debug( "Hello World! {Message}", message );
			Assert.Equal( 2, history.Events.Count() );
		}
	}

	class ConfigureLoggerConfigurationsCommand : AssignConfigurationCommand<ITransformer<LoggerConfiguration>[]>
	{
		public static ConfigureLoggerConfigurationsCommand Instance { get; } = new ConfigureLoggerConfigurationsCommand();
		ConfigureLoggerConfigurationsCommand() : base( LoggerConfigurationsConfiguration.Instance ) {}
	}

	class RecordingLoggerConfigurationsFactory : LoggerConfigurationsFactory
	{
		public new static RecordingLoggerConfigurationsFactory Instance { get; } = new RecordingLoggerConfigurationsFactory();

		protected override IEnumerable<ITransformer<LoggerConfiguration>> From( object parameter )
		{
			foreach ( var transformer in base.From( parameter ) )
			{
				yield return transformer;
			}

			yield return new HistoryTransform( LoggerHistoryConfiguration.Instance.Get( parameter ) );
		}

		class HistoryTransform : TransformerBase<LoggerConfiguration>
		{
			readonly ILoggerHistory history;

			public HistoryTransform( ILoggerHistory history )
			{
				this.history = history;
			}

			public override LoggerConfiguration Create( LoggerConfiguration parameter ) => parameter.WriteTo.Sink( history );
		}
	}
	
	class LoggerHistoryConfiguration : WritableParameterizedConfiguration<ILoggerHistory>
	{
		public static LoggerHistoryConfiguration Instance { get; } = new LoggerHistoryConfiguration();
		LoggerHistoryConfiguration() : base( o => new LoggerHistorySink() ) {}
	}

	class LoggingLevelSwitchConfiguration : WritableParameterizedConfiguration<LoggingLevelSwitch>
	{
		public static LoggingLevelSwitchConfiguration Instance { get; } = new LoggingLevelSwitchConfiguration();
		LoggingLevelSwitchConfiguration() : base( Factory.Instance.Create ) {}

		class Factory : FactoryBase<object, LoggingLevelSwitch>
		{
			public static Factory Instance { get; } = new Factory();
			Factory() {}

			public override LoggingLevelSwitch Create( object parameter ) => new LoggingLevelSwitch( MinimumLevelConfiguration.Instance.Get( parameter ) );
		}
	}

	class LoggerConfigurationConfiguration : WritableParameterizedConfiguration<LoggerConfiguration>
	{
		public static LoggerConfigurationConfiguration Instance { get; } = new LoggerConfigurationConfiguration();
		LoggerConfigurationConfiguration() : base( LoggerConfigurationFactory.Instance.Create ) {}

		class LoggerConfigurationFactory : FactoryBase<object, LoggerConfiguration>
		{
			public static LoggerConfigurationFactory Instance { get; } = new LoggerConfigurationFactory();
			LoggerConfigurationFactory() {}

			public override LoggerConfiguration Create( object parameter ) => 
				LoggerConfigurationsConfiguration.Instance.Get( parameter ).Aggregate( new LoggerConfiguration(), ( configuration, transformer ) => transformer.Create( configuration ) );
		}
	}

	class LoggerCongfiguration : WritableParameterizedConfiguration<ILogger>
	{
		public static LoggerCongfiguration Instance { get; } = new LoggerCongfiguration();
		LoggerCongfiguration() : base( Factory.Instance.Create ) {}

		class Factory : FactoryBase<object, ILogger>
		{
			public static Factory Instance { get; } = new Factory();
			Factory() {}

			public override ILogger Create( object parameter ) => LoggerConfigurationConfiguration.Instance.Get( parameter ).CreateLogger().ForSource( parameter );
		}
	}

	class LoggerConfigurationsConfiguration : WritableParameterizedConfiguration<ITransformer<LoggerConfiguration>[]>
	{
		public static LoggerConfigurationsConfiguration Instance { get; } = new LoggerConfigurationsConfiguration();
		LoggerConfigurationsConfiguration() : base( LoggerConfigurationsFactory.Instance.Create ) {}
	}

	public class LoggerConfigurationsFactory : FactoryBase<object, ITransformer<LoggerConfiguration>[]>
	{
		public static LoggerConfigurationsFactory Instance { get; } = new LoggerConfigurationsFactory();
		protected LoggerConfigurationsFactory() {}

		public override ITransformer<LoggerConfiguration>[] Create( object parameter ) => From( parameter ).ToArray();

		protected virtual IEnumerable<ITransformer<LoggerConfiguration>> From( object parameter )
		{
			yield return new ControllerTransform( LoggingLevelSwitchConfiguration.Instance.Get( parameter ) );
			yield return new CreatorFilterTransformer();
		}

		class ControllerTransform : TransformerBase<LoggerConfiguration>
		{
			readonly LoggingLevelSwitch controller;
			public ControllerTransform( LoggingLevelSwitch controller )
			{
				this.controller = controller;
			}

			public override LoggerConfiguration Create( LoggerConfiguration parameter ) => parameter.MinimumLevel.ControlledBy( controller );
		}
	}
}
