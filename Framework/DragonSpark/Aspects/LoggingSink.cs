using DragonSpark.Application;
using DragonSpark.Aspects.Build;
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
using PostSharp.Aspects;
using PostSharp.Extensibility;
using Serilog.Core;
using Serilog.Debugging;
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
			Diagnostics.LoggerConfigurations.Configure.Instance.WithParameter( DefaultSystemLoggerConfigurations.Default.Concat( LoggerConfigurations.Default ).Accept ),
			DisposeOnCompleteCommand.Default,
			ConfigureSelfLog.Default
		) {}
	}

	public sealed class ConfigureSelfLog : RunCommandBase
	{
		public static ConfigureSelfLog Default { get; } = new ConfigureSelfLog();
		ConfigureSelfLog() {}

		public override void Execute()
		{
			SelfLog.Enable( Emit.Instance.Execute );
			Disposables.Default.Add( new DisposableAction( SelfLog.Disable ) );
		}

		public sealed class Emit : CommandBase<string>
		{
			public static Emit Instance { get; } = new Emit();
			Emit() {}

			public override void Execute( string parameter ) => LoggingSink.Default.Execute( this, SeverityType.Error, "The PostSharp SelfLog encountered a problem: {0}", parameter );
		}
	}

	public sealed class DisposeOnCompleteCommand : RunCommandBase
	{
		public static DisposeOnCompleteCommand Default { get; } = new DisposeOnCompleteCommand();
		DisposeOnCompleteCommand() : this( AspectRepositoryService.Default, Disposables.Default ) {}

		readonly IAspectRepositoryService service;
		readonly IDisposable disposable;
		readonly EventHandler onAction;

		public DisposeOnCompleteCommand( IAspectRepositoryService service, IDisposable disposable )
		{
			this.service = service;
			this.disposable = disposable;
			onAction = DefaultOnAspectDiscoveryCompleted;
		}

		public override void Execute() => service.AspectDiscoveryCompleted += onAction;

		void DefaultOnAspectDiscoveryCompleted( object sender, EventArgs eventArgs )
		{
			service.AspectDiscoveryCompleted -= onAction;
			disposable.Dispose();
		}
	}

	public sealed class TypeDefinitionFormatter : IFormattable
	{
		readonly ITypeDefinition definition;
		public TypeDefinitionFormatter( ITypeDefinition definition )
		{
			this.definition = definition;
		}

		public string ToString( string format = null, IFormatProvider formatProvider = null ) => definition.ReferencedType.FullName;
	}

	sealed class LoggerConfigurations : ItemSource<ILoggingConfiguration>
	{
		public static LoggerConfigurations Default { get; } = new LoggerConfigurations();
		LoggerConfigurations() : base( new AddSinkConfiguration( LoggingSink.Default ) ) {}
	}

	public class LoggingSink : DelegatedCommand<Message>, ILogEventSink
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

	public static class Extensions
	{
		public static void Execute( this ICommand<Message> @this, object element, string format, params object[] arguments )
			=> @this.Execute( MessageLocation.Of( element ), format, arguments );

		public static void Execute( this ICommand<Message> @this, MessageLocation messageLocation, string format, params object[] arguments )
			=> Execute( @this, messageLocation, SeverityType.Info, format, arguments );

		public static void Execute( this ICommand<Message> @this, object element, SeverityType severity, string format, params object[] arguments )
			=> Execute( @this, MessageLocation.Of( element ), severity, format, arguments );

		public static void Execute( this ICommand<Message> @this, MessageLocation messageLocation, SeverityType severity, string format, params object[] arguments ) => 
			@this.Execute( new Message( messageLocation, severity, TextHasher.Default.Get( format ), string.Format( format, arguments ), null, null, null ) );
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
