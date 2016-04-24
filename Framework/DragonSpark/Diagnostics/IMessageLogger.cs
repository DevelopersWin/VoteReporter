using DragonSpark.Activation;
using DragonSpark.Extensions;
using DragonSpark.Runtime;
using DragonSpark.Runtime.Specifications;
using DragonSpark.Setup;
using PostSharp.Patterns.Contracts;
using Serilog;
using Serilog.Configuration;
using Serilog.Core;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Markup;

namespace DragonSpark.Diagnostics
{
	[ContentProperty( nameof(Commands) )]
	public class ConfiguringLoggerConfigurationFactory : TransformerBase<LoggerConfiguration>
	{
		public Collection<Command<LoggerConfiguration>> Commands { get; } = new Collection<Command<LoggerConfiguration>>();

		protected override LoggerConfiguration CreateItem( LoggerConfiguration configuration ) => Commands.Aggregate( configuration, ( loggerConfiguration, command ) => loggerConfiguration.With( command.Run ) );
	}

	public class AddTextWriterCommand : AddSinkCommand
	{
		public AddTextWriterCommand() : this( new StringWriter(), "{Timestamp} [{Level}] {Message}{NewLine}{Exception}" ) {}

		public AddTextWriterCommand( [Required]TextWriter writer, string outputTemplate )
		{
			Writer = writer;
			OutputTemplate = outputTemplate;
		}

		public TextWriter Writer { get; set; }

		public string OutputTemplate { get; set; }

		protected override void Configure( LoggerSinkConfiguration configuration ) => configuration.TextWriter( Writer, RestrictedToMinimumLevel, OutputTemplate, FormatProvider );
	}

	public abstract class AddSinkCommand : LoggerConfigurationCommandBase<LoggerSinkConfiguration>
	{
		protected AddSinkCommand() : this( LogEventLevel.Verbose ) {}

		protected AddSinkCommand( LogEventLevel restrictedToMinimumLevel ) : base( configuration => configuration.WriteTo )
		{
			RestrictedToMinimumLevel = restrictedToMinimumLevel;
		}

		public IFormatProvider FormatProvider { get; set; }

		public LogEventLevel RestrictedToMinimumLevel { get; set; }
	}

	public abstract class MinimumLevelCommandBase : LoggerConfigurationCommandBase<LoggerMinimumLevelConfiguration>
	{
		protected MinimumLevelCommandBase() : base( configuration => configuration.MinimumLevel ) {}
	}

	public class MinimumLevelSwitchCommand : MinimumLevelCommandBase
	{
		public MinimumLevelSwitchCommand() : this( LogEventLevel.Information ) {}

		public MinimumLevelSwitchCommand( LogEventLevel level ) : this( new Serilog.Core.LoggingLevelSwitch( level ) ) {}

		public MinimumLevelSwitchCommand( [Required]Serilog.Core.LoggingLevelSwitch controller )
		{
			Controller = controller;
		}

		[Required]
		public Serilog.Core.LoggingLevelSwitch Controller { [return: Required]get; set; }

		protected override void Configure( LoggerMinimumLevelConfiguration configuration ) => configuration.ControlledBy( Controller );
	}

	public abstract class ReadFromCommandBase : LoggerConfigurationCommandBase<LoggerSettingsConfiguration>
	{
		protected ReadFromCommandBase() : base( configuration => configuration.ReadFrom ) {}
	}

	public class ReadFromSettingsCommand : ReadFromCommandBase
	{
		public ReadFromSettingsCommand( [Required]ILoggerSettings settings )
		{
			Settings = settings;
		}

		[Required]
		public ILoggerSettings Settings { [return: Required]get; set; }

		protected override void Configure( LoggerSettingsConfiguration configuration ) => configuration.Settings( Settings );
	}

	public class ReadFromKeyValuePairsCommand : ReadFromCommandBase
	{
		public ReadFromKeyValuePairsCommand() : this( new Dictionary<string, string>() ) {}

		public ReadFromKeyValuePairsCommand( [Required]IDictionary<string, string> dictionary )
		{
			Dictionary = dictionary;
		}

		[Required]
		public IDictionary<string, string> Dictionary { [return: Required]get; set; }

		protected override void Configure( LoggerSettingsConfiguration configuration ) => configuration.KeyValuePairs( Dictionary );
	}

	public abstract class DestructureCommandBase : LoggerConfigurationCommandBase<LoggerDestructuringConfiguration>
	{
		protected DestructureCommandBase() : base( configuration => configuration.Destructure ) {}
	}

	public class DestructureMethodCommand : DestructureByFactoryCommand<MethodBase>
	{
		public static DestructureMethodCommand Instance { get; } = new DestructureMethodCommand();

		public DestructureMethodCommand() : base( MethodFormatter.Instance ) {}
	}

	public abstract class DestructureByFactoryCommand<TParameter> : DestructureCommandBase
	{
		protected DestructureByFactoryCommand( IFactory<TParameter, object> factory )
		{
			Factory = factory;
		}

		public IFactory<TParameter, object> Factory { get; set; }

		protected override void Configure( LoggerDestructuringConfiguration configuration ) => configuration.ByTransforming( Factory.ToDelegate() );
	}

	[ContentProperty( nameof(Policies) )]
	public class DestructureCommand : DestructureCommandBase
	{
		public Collection<IDestructuringPolicy> Policies { get; } = new Collection<IDestructuringPolicy>();

		protected override void Configure( LoggerDestructuringConfiguration configuration ) => configuration.With( Policies.Fixed() );
	}

	public class DestructureTypeCommand : DestructureCommandBase
	{
		[Required]
		public Type ScalarType { [return: Required]get; set; }

		protected override void Configure( LoggerDestructuringConfiguration configuration ) => configuration.AsScalar( ScalarType );
	}

	public class DestructureMaximumDepthCommand : DestructureCommandBase
	{
		[Range( 0, 100 )]
		public int MaximumDepth { get; set; }

		protected override void Configure( LoggerDestructuringConfiguration configuration ) => configuration.ToMaximumDepth( MaximumDepth );
	}

	public abstract class DestructureByTranformingCommandBase<T> : DestructureCommandBase
	{
		protected DestructureByTranformingCommandBase( [Required]IFactory<T, object> factory )
		{
			Factory = factory;
		}

		[Required]
		public IFactory<T, object> Factory { [return: Required]get; set; }

		protected override void Configure( LoggerDestructuringConfiguration configuration ) => configuration.ByTransforming<T>( Factory.Create );
	}

	public abstract class FilterCommandBase : LoggerConfigurationCommandBase<LoggerFilterConfiguration>
	{
		protected FilterCommandBase() : base( configuration => configuration.Filter ) {}
	}

	[ContentProperty( nameof(Items) )]
	public class FilterCommand : FilterCommandBase
	{
		public Collection<ILogEventFilter> Items { get; } = new Collection<ILogEventFilter>();

		protected override void Configure( LoggerFilterConfiguration configuration ) => configuration.With( Items.Fixed() );
	}

	public class FilterByIncludingOnlyCommand : FilterBySpecificationCommandBase
	{
		protected override void Configure( LoggerFilterConfiguration configuration ) => configuration.ByIncludingOnly( Specification.IsSatisfiedBy );
	}

	public abstract class FilterBySpecificationCommandBase : FilterCommandBase
	{
		[Required]
		public ISpecification<LogEvent> Specification { [return: Required]get; set; }
	}

	public class FilterByExcludingCommand : FilterBySpecificationCommandBase
	{
		protected override void Configure( LoggerFilterConfiguration configuration ) => configuration.ByExcluding( Specification.IsSatisfiedBy );
	}

	[ContentProperty( nameof(Items) )]
	public class EnrichCommand : EnrichCommandBase
	{
		public Collection<ILogEventEnricher> Items { get; } = new Collection<ILogEventEnricher>();
		
		protected override void Configure( LoggerEnrichmentConfiguration configuration ) => configuration.With( Items.Fixed() );
	}

	public class EnrichWithPropertyCommand : EnrichCommandBase
	{
		public string PropertyName { get; set; }

		public object Value { get; set; }

		public bool DestructureObjects { get; set; }

		protected override void Configure( LoggerEnrichmentConfiguration configuration ) => configuration.WithProperty( PropertyName, Value, DestructureObjects );
	}

	public abstract class EnrichCommandBase : LoggerConfigurationCommandBase<LoggerEnrichmentConfiguration>
	{
		protected EnrichCommandBase() : base( configuration => configuration.Enrich ) {}
	}

	public class MinimumLevelIsCommand : MinimumLevelCommandBase
	{
		public MinimumLevelIsCommand() : this( LogEventLevel.Information ) {}

		public MinimumLevelIsCommand( LogEventLevel level )
		{
			Level = level;
		}

		public LogEventLevel Level { get; set; }

		protected override void Configure( LoggerMinimumLevelConfiguration configuration ) => configuration.Is( Level );
	}

	public abstract class LoggerConfigurationCommandBase<T> : SetupCommandBase<LoggerConfiguration>
	{
		readonly Func<LoggerConfiguration, T> transform;

		protected LoggerConfigurationCommandBase( Func<LoggerConfiguration, T> transform )
		{
			this.transform = transform;
		}

		protected sealed override void OnExecute( LoggerConfiguration parameter )
		{
			var transformed = transform( parameter );
			Configure( transformed );
		}

		protected abstract void Configure( T configuration );
	}
}