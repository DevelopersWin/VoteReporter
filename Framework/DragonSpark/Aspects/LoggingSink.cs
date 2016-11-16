using DragonSpark.Application;
using DragonSpark.Aspects.Definitions;
using DragonSpark.Commands;
using DragonSpark.Diagnostics;
using DragonSpark.Diagnostics.Configurations;
using DragonSpark.Extensions;
using DragonSpark.Runtime;
using DragonSpark.Sources;
using DragonSpark.Sources.Parameterized;
using DragonSpark.Sources.Parameterized.Caching;
using DragonSpark.Sources.Scopes;
using DragonSpark.Specifications;
using DragonSpark.TypeSystem;
using JetBrains.Annotations;
using PostSharp;
using PostSharp.Extensibility;
using Serilog.Core;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace DragonSpark.Aspects
{
	public sealed class InitializationCommand : SpecificationCommand<object>
	{
		public static InitializationCommand Default { get; } = new InitializationCommand();
		InitializationCommand() : base( new OnlyOnceSpecification(), Configurations.Default.Execute ) {}
	}

	sealed class Configurations : CompositeCommand
	{
		public static Configurations Default { get; } = new Configurations();

		Configurations( LogEventLevel minimumLevel = LogEventLevel.Verbose ) : base(
			AssignApplicationParts.Default.With( typeof(MethodFormatter), typeof(TypeFormatter), typeof(TypeDefinitionFormatter) ),
			MinimumLevelConfiguration.Default.ToCommand( minimumLevel ),
			Diagnostics.LoggerConfigurations.Configure.Instance.WithParameter( DefaultSystemLoggerConfigurations.Default.Concat( LoggerConfigurations.Default ).Accept )
		) {}
	}

	public sealed class TypeDefinitionFormatter : IFormattable
	{
		readonly ITypeDefinition definition;
		public TypeDefinitionFormatter( ITypeDefinition definition )
		{
			this.definition = definition;
		}

		public string ToString( string format, IFormatProvider formatProvider ) => definition.ReferencedType.FullName;
	}

	sealed class LoggerConfigurations : ItemSource<ILoggingConfiguration>
	{
		public static LoggerConfigurations Default { get; } = new LoggerConfigurations();
		LoggerConfigurations() : base( new AddSinkConfiguration( LoggingSink.Default ) ) {}
	}

	public class LoggingSink : DelegatedCommand<Message>, ILogEventSink
	{
		public static LoggingSink Default { get; } = new LoggingSink();
		LoggingSink() : this( MessageFactory.Default.Get, MessageSource.MessageSink.Write ) {}

		readonly Func<LogEvent, Message> source;
		readonly Action<Message> write;

		[UsedImplicitly]
		public LoggingSink( Func<LogEvent, Message> source, Action<Message> write ) : base( write )
		{
			this.source = source;
			this.write = Execute;
		}

		public void Emit( LogEvent logEvent ) => source( logEvent ).With( write );
	}


	public sealed class MessageFactory : ParameterizedSourceBase<LogEvent, Message>
	{
		public static MessageFactory Default { get; } = new MessageFactory();
		MessageFactory() : this( LogEventTextFactory.Default.Get, LevelMappings.Default.Get, TextHasher.Default.Get ) {}

		readonly Func<LogEvent, string> textSource;
		readonly Func<LogEventLevel, SeverityType> mappings;
		readonly Alter<string> hasher;
		
		[UsedImplicitly]
		public MessageFactory( Func<LogEvent, string> textSource, Func<LogEventLevel, SeverityType> mappings, Alter<string> hasher )
		{
			this.textSource = textSource;
			this.mappings = mappings;
			this.hasher = hasher;
		}

		public override Message Get( LogEvent parameter )
		{
			var source = parameter.Properties.ContainsKey( Constants.SourceContextPropertyName ) ? parameter.Properties[Constants.SourceContextPropertyName].As<ScalarValue>().With( MessageLocation.Of ) : null;
			var messageId = hasher( parameter.MessageTemplate.Text );
			var text = textSource( parameter );
			var level = mappings( parameter.Level );
			var result = new Message( source ?? MessageLocation.Unknown, level, messageId, text, null, null, parameter.Exception );
			return result;
		}
	}

	sealed class LevelMappings : DictionaryCache<LogEventLevel, SeverityType>
	{
		public static LevelMappings Default { get; } = new LevelMappings();
		LevelMappings() : base(
			new Dictionary<LogEventLevel, SeverityType>
			{
				{ LogEventLevel.Verbose, SeverityType.Info },
				{ LogEventLevel.Debug, SeverityType.ImportantInfo },
				{ LogEventLevel.Information, SeverityType.ImportantInfo },
				{ LogEventLevel.Warning, SeverityType.Warning },
				{ LogEventLevel.Error, SeverityType.Error },
				{ LogEventLevel.Fatal, SeverityType.Fatal },
			}.ToImmutableDictionary() 
		) {}
	}

	
}
