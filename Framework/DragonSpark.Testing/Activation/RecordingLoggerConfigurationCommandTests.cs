using DragonSpark.Activation;
using DragonSpark.Diagnostics;
using DragonSpark.Runtime;
using DragonSpark.Runtime.Properties;
using DragonSpark.Runtime.Stores;
using Ploeh.AutoFixture.Xunit2;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Xunit;

namespace DragonSpark.Testing.Activation
{
	public class RecordingLoggerConfigurationCommandTests
	{
		[Fact]
		public void BasicContext()
		{
			var store = new ConfigurationStore();
			RecordingLoggerConfigurationCommand.Instance.Execute( store );

			var level = store.Get<DefaultLevel>();
			var controller = store.Get<LoggingLevelSwitchSource>();

			var one = new object();
			var first = controller.Get( one );
			Assert.Same( first, controller.Get( one ) );

			Assert.Equal( LogEventLevel.Information, first.MinimumLevel );

			const LogEventLevel assigned = LogEventLevel.Debug;
			level.Assign( assigned );

			var two = new object();
			var second = controller.Get( two );
			Assert.NotSame( first, second );
			Assert.Same( second, controller.Get( two ) );

			Assert.Equal( assigned, second.MinimumLevel );
		}

		[Theory, AutoData]
		void VerifyHistory( ConfigurationStore store, object context, string message )
		{
			RecordingLoggerConfigurationCommand.Instance.Execute( store );

			Assert.Same( store.Get<LoggerHistorySource>(), store.Get<LoggerHistorySource>() );

			var history = store.Get<LoggerHistorySource>().Get( context );
			Assert.Empty( history.Events );
			Assert.Same( history, store.Get<LoggerHistorySource>().Get( context ) );

			var logger = store.Get<LoggerSource>().Get( context );
			Assert.Empty( history.Events );
			logger.Information( "Hello World! {Message}", message );
			Assert.Single( history.Events, item => item.RenderMessage().Contains( message ) );

			var level = store.Get<LoggingLevelSwitchSource>().Get( context );
			logger.Debug( "Hello World! {Message}", message );
			Assert.Single( history.Events );
			level.MinimumLevel = LogEventLevel.Debug;

			logger.Debug( "Hello World! {Message}", message );
			Assert.Equal( 2, history.Events.Count() );
		}
	}

	class RecordingLoggerConfigurationCommand : LoggerConfigurationCommand
	{
		public static RecordingLoggerConfigurationCommand Instance { get; } = new RecordingLoggerConfigurationCommand();
		RecordingLoggerConfigurationCommand() : base( Factory.Instance.Create ) {}

		class Factory : FactoryBase<ConfigurationStore, Configurations>
		{
			public static Factory Instance { get; } = new Factory();

			public override Configurations Create( ConfigurationStore parameter ) => 
				new ConfigurationsFactory( parameter.Get<LoggerHistorySource>().Get, parameter.Get<LoggingLevelSwitchSource>().Get ).Create;
		}

		new class ConfigurationsFactory : LoggerConfigurationCommand.ConfigurationsFactory
		{
			readonly Func<object, ILoggerHistory> history;
			
			public ConfigurationsFactory( Func<object, ILoggerHistory> history, Func<object, LoggingLevelSwitch> controller ) : base( controller )
			{
				this.history = history;
			}

			protected override IEnumerable<ITransformer<LoggerConfiguration>> From( object parameter )
			{
				foreach ( var transformer in base.From( parameter ) )
				{
					yield return transformer;
				}

				yield return new HistoryTransform( history( parameter ) );
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
	}

	abstract class LoggerConfigurationCommand : ConfigureConfigurationStoreCommand
	{
		public delegate ImmutableArray<ITransformer<LoggerConfiguration>> Configurations( object instance );

		readonly Func<ConfigurationStore, Configurations> configurationsSource;
		protected LoggerConfigurationCommand( Func<ConfigurationStore, Configurations> configurationsSource )
		{
			this.configurationsSource = configurationsSource;
		}

		public override void Execute( ConfigurationStore parameter )
		{
			var level = parameter.Get<DefaultLevel>();
			parameter.Get<LoggingLevelSwitchSource>().Assigned( new LoggingLevelSwitchFactory( level ).ToDelegate() );

			var value = new Func<object, ImmutableArray<ITransformer<LoggerConfiguration>>>( configurationsSource( parameter ) );
			var configurations = parameter.Get<LoggerConfigurationsSource>().Assigned( value );
			var configuration = parameter.Get<LoggerConfigurationSource>().Assigned( new LoggerConfigurationFactory( configurations.Get ).ToDelegate() );
			parameter.Get<LoggerSource>().Assigned( new LoggerFactory( configuration.Get ).ToDelegate() );
		}

		public class LoggingLevelSwitchFactory : FactoryBase<object, LoggingLevelSwitch>
		{
			readonly IStore<LogEventLevel> levelSource;

			public LoggingLevelSwitchFactory( IStore<LogEventLevel> levelSource )
			{
				this.levelSource = levelSource;
			}

			public override LoggingLevelSwitch Create( object parameter ) => new LoggingLevelSwitch( levelSource.Value );
		}

		public class ConfigurationsFactory : FactoryBase<object, ImmutableArray<ITransformer<LoggerConfiguration>>>
		{
			readonly Func<object, LoggingLevelSwitch> controller;
		
			public ConfigurationsFactory( Func<object, LoggingLevelSwitch> controller )
			{
				this.controller = controller;
			}

			public override ImmutableArray<ITransformer<LoggerConfiguration>> Create( object parameter ) => From( parameter ).ToImmutableArray();

			protected virtual IEnumerable<ITransformer<LoggerConfiguration>> From( object parameter )
			{
				yield return new ControllerTransform( controller( parameter ) );
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

		class LoggerConfigurationFactory : FactoryBase<object, LoggerConfiguration>
		{
			readonly Configurations configurations;

			public LoggerConfigurationFactory( Configurations configurations )
			{
				this.configurations = configurations;
			}

			public override LoggerConfiguration Create( object parameter ) => 
				configurations( parameter ).Aggregate( new LoggerConfiguration(), ( configuration, transformer ) => transformer.Create( configuration ) );
		}

		class LoggerFactory : FactoryBase<object, ILogger>
		{
			readonly Func<object, LoggerConfiguration> configurationSource;
			public LoggerFactory( Func<object, LoggerConfiguration> configurationSource )
			{
				this.configurationSource = configurationSource;
			}

			public override ILogger Create( object parameter ) => configurationSource( parameter ).CreateLogger().ForSource( parameter );
		}
	}

	abstract class ConfigureConfigurationStoreCommand : CommandBase<ConfigurationStore> {}

	class ConfigurationStore : Cache<Type, IWritableStore>
	{
		readonly static Func<Type, IWritableStore> Create = Constructor.Instance.CreateUsing<IWritableStore>;

		public ConfigurationStore() : base( Create ) {}

		public T Get<T>() where T : IWritableStore, new() => (T)Get( typeof(T) );
	}

	public interface IConfigurationStore<T> : IConfigurationStore<object, T> {}

	public interface IConfigurationStore<TKey, TValue> : IWritableStore<Func<TKey, TValue>>
	{
		TValue Get( TKey key );
	}

	class CachedConfigurationSource<T> : CachedConfigurationSource<object, T> where T : class
	{
		public CachedConfigurationSource( Func<object, T> reference ) : base( reference ) {}
	}

	class CachedConfigurationSource<TKey, TValue> : CachedConfigurationSourceBase<Cache<TKey, TValue>, TKey, TValue> where TKey : class where TValue : class
	{
		public CachedConfigurationSource( Func<TKey, TValue> reference ) : base( reference ) {}
	}

	abstract class CachedConfigurationSourceBase<TCache, TValue> : CachedConfigurationSourceBase<TCache, object, TValue>, IConfigurationStore<TValue> where TCache : class, ICache<object, TValue>
	{
		protected CachedConfigurationSourceBase( Func<object, TValue> reference ) : base( reference ) {}
	}

	abstract class CachedConfigurationSourceBase<TCache, TKey, TValue> : ConfigurationSource<TKey, TValue> where TCache : class, ICache<TKey, TValue>
	{
		protected CachedConfigurationSourceBase( Func<TKey, TValue> reference ) : base( reference ) {}

		protected override void OnAssign( Func<TKey, TValue> item ) => base.OnAssign( ParameterConstructor<Func<TKey, TValue>, TCache>.Default( item ).Get );
	}

	/*class ConfigurationSource<T> : ConfigurationSource<object, T>
	{
		public ConfigurationSource( Func<object, T> reference ) : base( reference ) {}
	}*/

	class ConfigurationSource<TKey, TValue> : FixedStore<Func<TKey, TValue>>, IConfigurationStore<TKey, TValue>
	{
		public ConfigurationSource( Func<TKey, TValue> reference ) : base( reference ) {}

		public TValue Get( TKey key ) => Value( key );
	}

	class DefaultLevel : FixedStore<LogEventLevel>
	{
		public DefaultLevel() : this( LogEventLevel.Information ) {}

		public DefaultLevel( LogEventLevel reference ) : base( reference ) {}
	}

	class LoggerHistorySource : CachedConfigurationSource<ILoggerHistory>
	{
		public LoggerHistorySource() : base( o => new LoggerHistorySink() ) {}
	}

	class LoggingLevelSwitchSource : CachedConfigurationSource<LoggingLevelSwitch>
	{
		public LoggingLevelSwitchSource() : this( o => new LoggingLevelSwitch() ) {}

		public LoggingLevelSwitchSource( Func<object, LoggingLevelSwitch> reference ) : base( reference ) {}
	}

	class LoggerConfigurationSource : CachedConfigurationSource<LoggerConfiguration>
	{
		public LoggerConfigurationSource() : this( o => new LoggerConfiguration() ) {}
		public LoggerConfigurationSource( Func<object, LoggerConfiguration> create ) : base( create ) {}
	}

	class LoggerSource : CachedConfigurationSource<ILogger>
	{
		public LoggerSource() : this( o => new LoggerConfiguration().CreateLogger() ) {}
		public LoggerSource( Func<object, ILogger> reference ) : base( reference ) {}
	}

	class LoggerConfigurationsSource : CachedConfigurationSourceBase<StoreCache<ImmutableArray<ITransformer<LoggerConfiguration>>>, ImmutableArray<ITransformer<LoggerConfiguration>>>
	{
		public LoggerConfigurationsSource() : this( o => ImmutableArray<ITransformer<LoggerConfiguration>>.Empty ) {}
		public LoggerConfigurationsSource( Func<object, ImmutableArray<ITransformer<LoggerConfiguration>>> factory ) : base( factory ) {}
	}
}
