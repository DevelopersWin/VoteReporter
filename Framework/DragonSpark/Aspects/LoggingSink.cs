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
using Serilog.Debugging;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace DragonSpark.Aspects
{
	public sealed class InitializationCommand : SpecificationCommand<object>
	{
		public static InitializationCommand Default { get; } = new InitializationCommand();
		InitializationCommand() : base( Common.Assigned.And( new OnlyOnceSpecification() ), new CompositeCommand( ConfigurationCommands.Default ).Execute ) {}

		public override void Execute( object parameter = null )
		{
			base.Execute( parameter );

			/*var log = Logger.Default.Get( this );
			for ( int i = 0; i < 1000; i++ )
			{
				log.Information( $"TESTING THIS: {i}" );
			}*/
		}
	}

	public sealed class ConfigurationCommands : ItemSourceBase<ICommand>
	{
		public static ConfigurationCommands Default { get; } = new ConfigurationCommands();
		ConfigurationCommands() : this( Configuration<DiagnosticsConfiguration>.Default.Get ) {}

		readonly Func<DiagnosticsConfiguration> source;

		public ConfigurationCommands( Func<DiagnosticsConfiguration> source )
		{
			this.source = source;
		}

		protected override IEnumerable<ICommand> Yield()
		{
			var configuration = source();
			if ( configuration != null )
			{
				throw new InvalidOperationException( "WTF!" );
				yield return MinimumLevelConfiguration.Default.ToCommand( configuration.MinimumLevel );
				yield return AssignApplicationParts.Default.With( configuration.RegisteredTypes.Fixed() );
				yield return LoggerConfigurations.Configure.Instance.WithParameter( DefaultSystemLoggerConfigurations.Default, configuration.Configurations.Fixed() );
				// yield return DisposeOnCompleteCommand.Default,
				yield return ConfigureSelfLog.Default;
			}
		}
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

	public sealed class DefaultConfigurationTypes : ItemSource<Type>
	{
		public static DefaultConfigurationTypes Default { get; } = new DefaultConfigurationTypes();
		DefaultConfigurationTypes() : base( typeof(DiagnosticsConfiguration), typeof(MethodFormatter), typeof(TypeFormatter), typeof(TypeDefinitionFormatter) ) {}
	}

	// [DataContract( Namespace = "clr-namespace:DragonSpark.Aspects;assembly:DragonSpark" )]
	public class DiagnosticsConfiguration
	{
		public LogEventLevel MinimumLevel { get; set; }
		public Collection<Type> RegisteredTypes { get; set; } = new Collection<Type>( DefaultConfigurationTypes.Default.ToList() );
		public Collection<ILoggingConfiguration> Configurations { get; set; } = new Collection<ILoggingConfiguration>( DefaultLoggerConfigurations.Default.ToList() );
	}

	public sealed class Configuration<T> : DelegatedSingletonSource<T>
	{
		readonly static Type Type = typeof(T);

		public static Configuration<T> Default { get; } = new Configuration<T>();
		Configuration() : this( Implementation.Instance.Get ) {}

		[UsedImplicitly]
		public Configuration( Func<T> factory ) : base( factory ) {}
		
		public sealed class Implementation : SourceBase<T>
		{
			public static Implementation Instance { get; } = new Implementation();
			Implementation() : this( PostSharpEnvironment.CurrentProject, Serializer.Default ) {}

			readonly IProject project;
			readonly ISerializer serializer;

			[UsedImplicitly]
			public Implementation( IProject project, ISerializer serializer )
			{
				this.project = project;
				this.serializer = serializer;
			}

			public override T Get()
			{
				var data = project.GetExtensionElements( Type.Name, $"clr-namespace:{Type.Namespace};assembly:{Type.Assembly().GetName().Name}" ).SingleOrDefault()?.Xml;
				var result = data != null ? serializer.Load<T>( data ) : default(T);
				return result;
			}
		}
	}

	/*public sealed class DisposeOnCompleteCommand : RunCommandBase
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

		public override void Execute()
		{
			/*dynamic currentProject = PostSharpEnvironment.CurrentProject;
			//dynamic temp = currentProject.GetType().GetRuntimeProperty( "ApplicationConfiguration" ).GetValue( null );
			//var types = currentProject.ProjectConfiguration.SectionTypes;

			// var argument = currentProject.GetExtensionElements( nameof(DiagnosticsConfiguration), "clr-namespace:DragonSpark.Aspects;assembly:DragonSpark" );
			//Message.Write( MessageLocation.Of( this ), SeverityType.Warning, "6776", "hello????? {0} - {1} - {2}", types.Count, types[0].LocalName, types[0].Namespace );
			service.AspectDiscoveryCompleted += onAction;

			var temp = (Array)PostSharpEnvironment.CurrentProject.GetType().GetRuntimeFields().Single( info => info.Name == "^jnC1KSwk" ).GetValue( PostSharpEnvironment.CurrentProject );
			foreach ( dynamic item in temp )
			{
				var enumerable = item.SectionTypes;
				foreach ( var sections in enumerable )
				{
					Message.Write( MessageLocation.Of( this ), SeverityType.Warning, "6776", "hello????? {0} - {1} - {2}", item.SectionTypes.Count, sections.LocalName, sections.Namespace );
				}
			}#1#
			
			// Message.Write( MessageLocation.Of( this ), SeverityType.Warning, "6776", "hello????? {0} - {1} - {2}", argument.Count(), xml, diagnosticsConfiguration );
		}

		void DefaultOnAspectDiscoveryCompleted( object sender, EventArgs eventArgs )
		{
			service.AspectDiscoveryCompleted -= onAction;
			disposable.Dispose();
			Message.Write( MessageLocation.Of( this ), SeverityType.Warning, "6776", "DISPOSE CALLED!!!" );
		}
	}*/

	public sealed class TypeDefinitionFormatter : IFormattable
	{
		readonly ITypeDefinition definition;
		public TypeDefinitionFormatter( ITypeDefinition definition )
		{
			this.definition = definition;
		}

		public string ToString( string format = null, IFormatProvider formatProvider = null ) => definition.ReferencedType.FullName;
	}

	sealed class DefaultLoggerConfigurations : ItemSource<ILoggingConfiguration>
	{
		public static DefaultLoggerConfigurations Default { get; } = new DefaultLoggerConfigurations();
		DefaultLoggerConfigurations() : base( new AddSinkConfiguration( LoggingSink.Default ) ) {}
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
