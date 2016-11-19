using System;
using DragonSpark.Diagnostics;
using DragonSpark.Extensions;
using DragonSpark.Runtime;
using DragonSpark.Sources.Parameterized;
using JetBrains.Annotations;
using PostSharp;
using PostSharp.Extensibility;
using Serilog.Core;
using Serilog.Events;

namespace DragonSpark.Aspects.Diagnostics
{
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
}