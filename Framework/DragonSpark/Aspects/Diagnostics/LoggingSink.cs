using DragonSpark.Commands;
using DragonSpark.Extensions;
using JetBrains.Annotations;
using PostSharp.Extensibility;
using Serilog.Core;
using Serilog.Events;
using System;

namespace DragonSpark.Aspects.Diagnostics
{
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
