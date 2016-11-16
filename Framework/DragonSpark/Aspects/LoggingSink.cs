using DragonSpark.Commands;
using DragonSpark.Extensions;
using DragonSpark.Runtime;
using DragonSpark.Sources.Parameterized;
using DragonSpark.Sources.Parameterized.Caching;
using DragonSpark.Specifications;
using JetBrains.Annotations;
using PostSharp;
using PostSharp.Extensibility;
using Serilog.Core;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace DragonSpark.Aspects
{
	public sealed class InitializationCommand : SpecificationCommand<object>
	{
		public static InitializationCommand Default { get; } = new InitializationCommand();
		InitializationCommand() : base( new OnlyOnceSpecification(), Implementation.Instance.Execute ) {}

		sealed class Implementation : RunCommandBase
		{
			public static Implementation Instance { get; } = new Implementation();
			Implementation() {}

			public override void Execute()
			{
				// Diagnostics.LoggerConfigurations.Default.Assign( DefaultSystemLoggerConfigurations.Default.Append( new LoggerConfiguration() ).Accept );
			}
		}

		/*sealed class LoggerConfigurations : ItemSourceBase<IAlteration<LoggerConfiguration>>
		{
			protected override IEnumerable<IAlteration<LoggerConfiguration>> Yield()
			{
				yield return new addsink;
			}
		}*/
	}

	public class LoggingSink : DelegatedCommand<Message>, ILogEventSink
	{
		public static LoggingSink Default { get; } = new LoggingSink();
		LoggingSink() : this( MessageFactory.Default.Get, MessageSource.MessageSink.Write ) {}

		readonly Func<LogEvent, Message> source;
		readonly Action<Message> write;

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
		MessageFactory() : this( LevelMappings.Default.Get, TextHasher.Default.Get ) {}

		readonly Func<LogEventLevel, SeverityType> mappings;
		readonly Alter<string> hasher;
		
		[UsedImplicitly]
		public MessageFactory( Func<LogEventLevel, SeverityType> mappings, Alter<string> hasher )
		{
			this.mappings = mappings;
			this.hasher = hasher;
		}

		public override Message Get( LogEvent parameter )
		{
			var source = parameter.Properties.ContainsKey( Constants.SourceContextPropertyName ) ? parameter.Properties[Constants.SourceContextPropertyName].As<ScalarValue>().With( MessageLocation.Of ) : null;
			var messageId = hasher( parameter.MessageTemplate.Text );
			var text = parameter.RenderMessage();
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
				{ LogEventLevel.Verbose, SeverityType.Verbose },
				{ LogEventLevel.Debug, SeverityType.Debug },
				{ LogEventLevel.Information, SeverityType.Info },
				{ LogEventLevel.Warning, SeverityType.Warning },
				{ LogEventLevel.Error, SeverityType.Error },
				{ LogEventLevel.Fatal, SeverityType.Fatal },
			}.ToImmutableDictionary() 
		) {}
	}

	
}
