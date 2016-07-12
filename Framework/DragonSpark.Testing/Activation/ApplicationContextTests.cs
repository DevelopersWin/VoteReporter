using DragonSpark.Activation;
using DragonSpark.Diagnostics;
using DragonSpark.Runtime;
using DragonSpark.Runtime.Properties;
using DragonSpark.Runtime.Stores;
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
	public class ApplicationContextTests
	{
		[Fact]
		public void BasicContext()
		{
			// var context = new WindowsTestingApplicationContext();

			var store = new ConfigurationStore();
			RecordingLoggerConfigurationCommand.Instance.Execute( store );

			var level = store.Get<DefaultLevel>();
			var controller = store.Get<LoggingLevelSwitchStore>();

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
			// var cache = new ConfigurableCache<object, LogEventLevel>(  );
		}

		[Fact]
		public void ComplexContext()
		{
			
		}
	}

	class RecordingLoggerConfigurationCommand : LoggerConfigurationCommand
	{
		public static RecordingLoggerConfigurationCommand Instance { get; } = new RecordingLoggerConfigurationCommand();
		RecordingLoggerConfigurationCommand() : base( Factory.Instance.Create ) {}

		class Factory : FactoryBase<ConfigurationStore, Func<object, ImmutableArray<ITransformer<LoggerConfiguration>>>>
		{
			public static Factory Instance { get; } = new Factory();

			public override Func<object, ImmutableArray<ITransformer<LoggerConfiguration>>> Create( ConfigurationStore parameter ) => 
				new ConfigurationsFactory( parameter.Get<LoggerHistoryStore>().Get, parameter.Get<LoggingLevelSwitchStore>().Get ).Create;
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
		readonly Func<ConfigurationStore, Func<object, ImmutableArray<ITransformer<LoggerConfiguration>>>> configurationsSource;
		protected LoggerConfigurationCommand( Func<ConfigurationStore, Func<object, ImmutableArray<ITransformer<LoggerConfiguration>>>> configurationsSource )
		{
			this.configurationsSource = configurationsSource;
		}

		public override void Execute( ConfigurationStore parameter )
		{
			var level = parameter.Get<DefaultLevel>();
			parameter.Get<LoggingLevelSwitchStore>().Assigned( new LoggingLevelSwitchFactory( level ).ToDelegate() );

			var configurations = parameter.Get<LoggerConfigurationsStore>().Assigned( configurationsSource( parameter ) );
			var configuration = parameter.Get<LoggerConfigurationStore>().Assigned( new LoggerConfigurationFactory( configurations.Get ).ToDelegate() );
			parameter.Get<LoggerStore>().Assigned( new LoggerFactory( configuration.Get ).ToDelegate() );
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

		public class LoggerConfigurationFactory : FactoryBase<object, LoggerConfiguration>
		{
			readonly Func<object, ImmutableArray<ITransformer<LoggerConfiguration>>> configurations;

			public LoggerConfigurationFactory( Func<object, ImmutableArray<ITransformer<LoggerConfiguration>>> configurations )
			{
				this.configurations = configurations;
			}

			public override LoggerConfiguration Create( object parameter ) => configurations( parameter ).Aggregate( new LoggerConfiguration(), ( configuration, transformer ) => transformer.Create( configuration ) );
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

	class ConfigurationStore<T> : ConfigurationStore<object, T>, IConfigurationStore<T>
	{
		public ConfigurationStore( Func<object, T> reference ) : base( reference ) {}
	}

	class ConfigurationStore<TKey, TValue> : FixedStore<Func<TKey, TValue>>, IConfigurationStore<TKey, TValue>
	{
		public ConfigurationStore( Func<TKey, TValue> reference ) : base( reference ) {}

		public TValue Get( TKey key ) => Value( key );
	}

	class DefaultLevel : FixedStore<LogEventLevel>
	{
		public DefaultLevel() : this( LogEventLevel.Information ) {}

		public DefaultLevel( LogEventLevel reference ) : base( reference ) {}
	}

	class LoggerHistoryStore : ConfigurationStore<ILoggerHistory>
	{
		public LoggerHistoryStore() : base( o => new LoggerHistorySink() ) {}
	}

	class LoggingLevelSwitchStore : ConfigurationStore<LoggingLevelSwitch>
	{
		public LoggingLevelSwitchStore() : this( o => new LoggingLevelSwitch() ) {}

		public LoggingLevelSwitchStore( Func<object, LoggingLevelSwitch> reference ) : base( reference ) {}

		protected override void OnAssign( Func<object, LoggingLevelSwitch> item ) => base.OnAssign( new Cache<LoggingLevelSwitch>( item ).Get );
	}

	class LoggerConfigurationStore : ConfigurationStore<LoggerConfiguration>
	{
		public LoggerConfigurationStore() : this( o => new LoggerConfiguration() ) {}
		public LoggerConfigurationStore( Func<object, LoggerConfiguration> create ) : base( create ) {}

		protected override void OnAssign( Func<object, LoggerConfiguration> item ) => base.OnAssign( new Cache<LoggerConfiguration>( item ).Get );
	}

	class LoggerStore : ConfigurationStore<ILogger>
	{
		public LoggerStore() : this( o => new LoggerConfiguration().CreateLogger() ) {}
		public LoggerStore( Func<object, ILogger> reference ) : base( reference ) {}

		protected override void OnAssign( Func<object, ILogger> item ) => base.OnAssign( new Cache<ILogger>( item ).Get );
	}

	class LoggerConfigurationsStore : ConfigurationStore<ImmutableArray<ITransformer<LoggerConfiguration>>>
	{
		public LoggerConfigurationsStore() : this( o => ImmutableArray<ITransformer<LoggerConfiguration>>.Empty ) {}
		public LoggerConfigurationsStore( Func<object, ImmutableArray<ITransformer<LoggerConfiguration>>> factory ) : base( factory ) {}

		protected override void OnAssign( Func<object, ImmutableArray<ITransformer<LoggerConfiguration>>> item ) => base.OnAssign( new StoreCache<ImmutableArray<ITransformer<LoggerConfiguration>>>( item ).Get );
	}

	/*public class ConfigurableLoggerFactory : FactoryBase<object, ILogger>
	{
		readonly IConfigurableCache<ILoggerHistory> history;
		readonly IConfigurableCache<LoggingLevelSwitch> controller;
		readonly IConfigurableCache<ImmutableArray<ITransformer<LoggerConfiguration>>> configurations;

		public ConfigurableLoggerFactory( IConfigurableCache<ILoggerHistory> history, IConfigurableCache<LoggingLevelSwitch> controller, IConfigurableCache<ImmutableArray<ITransformer<LoggerConfiguration>>> configurations )
		{
			this.history = history;
			this.controller = controller;
			this.configurations = configurations;
		}

		// LoggingLevelSwitch Create( object instance ) => new LoggingLevelSwitch( Level.Get( instance ) );


		// public IConfigurableCache<object, LogEventLevel> Level { get; } = new StoreCache<LogEventLevel>().Configurable();
		

		// public IConfigurableCache<object, LoggingLevelSwitch> Controller { get; }
		

		/*public ICache<ILoggerHistory> History { get; } = new Cache<ILoggerHistory>( o => new LoggerHistorySink() );

		public ICache<ImmutableArray<ITransformer<LoggerConfiguration>>> Transformers { get; } = new StoreCache<ImmutableArray<ITransformer<LoggerConfiguration>>>( o => ImmutableArray.Create<ITransformer<LoggerConfiguration>>() );

		#1#
		public override ILogger Create( object parameter )
		{
			return new RecordingLoggerFactory( history.Get( parameter ), controller.Get( parameter ), configurations.Get( parameter ).ToArray() ).Create().ForSource( parameter );
		}
	}*/

	class WindowsTestingApplicationContext : ApplicationContext
	{
		public WindowsTestingApplicationContext() : base( Windows.Configure.Instance/*, LoadPartsCommand.Instance*/ ) {}

		/*class LoadPartsCommand : FixedCommand
		{
			public static LoadPartsCommand Instance { get; } = new LoadPartsCommand();

			LoadPartsCommand() : base( LoadPartAssemblyCommand.Instance, typeof(LoadPartsCommand).Assembly ) {}
		}*/
	}
}