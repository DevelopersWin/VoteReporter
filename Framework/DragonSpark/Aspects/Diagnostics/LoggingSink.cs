using DragonSpark.Commands;
using DragonSpark.Extensions;
using JetBrains.Annotations;
using PostSharp.Extensibility;
using Serilog.Core;
using Serilog.Events;
using System;

namespace DragonSpark.Aspects.Diagnostics
{
	public sealed class LoggingSink : DelegatedCommand<Message>, ILogEventSink
	{
		public static LoggingSink Default { get; } = new LoggingSink();
		LoggingSink() : this( MessageFactory.Default.Get, Message.Write ) {}

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
}
