using DragonSpark.Activation;
using DragonSpark.Diagnostics;
using DragonSpark.Diagnostics.Logger;
using DragonSpark.Extensions;
using DragonSpark.Testing.Framework;
using DragonSpark.Testing.Framework.Parameters;
using DragonSpark.Testing.Framework.Setup;
using Serilog;
using System;
using System.Composition;
using System.Linq;
using System.Reflection;
using Xunit;

namespace DragonSpark.Testing.Diagnostics
{
	[Trait( Traits.Category, Traits.Categories.ServiceLocation )]
	public class LoggerFactoryTests
	{
		[Theory, AutoData, AdditionalTypes( typeof(MethodFormatter) )]
		public void EnsureComposition( [Service]CompositionContext context, string text )
		{
			var logger = context.GetExport<ILogger>();
			
			Assert.Same( DefaultServiceProvider.Instance.Get<ILogger>(), logger );

			var method = new Action( AnotherMethod ).Method;
			var command = new LogCommand( logger );

			command.Execute( new HelloWorld( text, method ) );
			
			var history = context.GetExport<ILoggerHistory>();
			Assert.Same( DefaultServiceProvider.Instance.Get<ILoggerHistory>(), history );
			var message = LogEventMessageFactory.Instance.Create( history.Events ).Last();
			Assert.Contains( text, message );
			
			Assert.Contains( new MethodFormatter( method ).ToString( null, null ), message );
		}

		void AnotherMethod() {}

		class HelloWorld : LoggerTemplate
		{
			public HelloWorld( string text, MethodBase method ) : base( "Hello World! {Text} - {Method}", text, method ) {}
		}

		/*[Export]
		class LoggerFactory : DragonSpark.Diagnostics.LoggerFactory
		{
			/*public LoggerFactory() : this( new LoggingLevelSwitch() ) {}

			public LoggerFactory( LoggingLevelSwitch logging ) : base( new Factory( logging ).Create ) {}#1#
			[ImportingConstructor]
			public LoggerFactory( Func<LoggerConfiguration> configurationSource ) : base( configurationSource ) {}
		}

		[Export]
		class Factory : RecordingLoggerConfigurationFactory
		{
			[ImportingConstructor]
			public Factory( ILoggerHistory history, LoggingLevelSwitch controller ) : base( history, controller, 
				new ICommand<LoggerConfiguration>[] { /*DestructureMethodCommand.Instance,#1# EnrichFromLogContextCommand.Instance }.Select( command => new ConfiguringTransformer<LoggerConfiguration>( command.Execute ) ).Fixed()  
				) {}
		}*/
	}
}