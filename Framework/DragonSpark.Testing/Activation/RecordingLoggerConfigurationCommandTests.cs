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
			LoggerConfigurationsConfiguration.Instance.Assign( RecordingLoggerConfigurationsFactory.Instance.Create );

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
			LoggerConfigurationsConfiguration.Instance.Assign( RecordingLoggerConfigurationsFactory.Instance.Create );

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
	
	class LoggerHistoryConfiguration : ParameterizedConfiguration<ILoggerHistory>
	{
		public static LoggerHistoryConfiguration Instance { get; } = new LoggerHistoryConfiguration();
		LoggerHistoryConfiguration() : base( o => new LoggerHistorySink() ) {}
	}

	class LoggingLevelSwitchConfiguration : ParameterizedConfiguration<LoggingLevelSwitch>
	{
		public static LoggingLevelSwitchConfiguration Instance { get; } = new LoggingLevelSwitchConfiguration();
		LoggingLevelSwitchConfiguration() : base( o => new LoggingLevelSwitch( MinimumLevelConfiguration.Instance.Get( o ) ) ) {}
	}

	class LoggerConfigurationConfiguration : ParameterizedConfiguration<LoggerConfiguration>
	{
		public static LoggerConfigurationConfiguration Instance { get; } = new LoggerConfigurationConfiguration();
		LoggerConfigurationConfiguration() : base( o => LoggerConfigurationsConfiguration.Instance.Get( o ).Aggregate( new LoggerConfiguration(), ( configuration, transformer ) => transformer.Create( configuration ) ) ) {}
	}

	class LoggerCongfiguration : ParameterizedConfiguration<ILogger>
	{
		public static LoggerCongfiguration Instance { get; } = new LoggerCongfiguration();
		LoggerCongfiguration() : base( o => LoggerConfigurationConfiguration.Instance.Get( o ).CreateLogger().ForSource( o ) ) {}
	}

	class LoggerConfigurationsConfiguration : ParameterizedConfiguration<ITransformer<LoggerConfiguration>[]>
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
