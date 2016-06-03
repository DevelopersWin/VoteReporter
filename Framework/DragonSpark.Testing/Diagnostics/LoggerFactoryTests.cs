﻿using DragonSpark.Activation;
using DragonSpark.Diagnostics;
using DragonSpark.Diagnostics.Logger;
using DragonSpark.Extensions;
using DragonSpark.Runtime;
using DragonSpark.Setup;
using DragonSpark.Testing.Framework;
using DragonSpark.Testing.Framework.Setup;
using DragonSpark.Windows.Diagnostics;
using DragonSpark.Windows.Runtime;
using Serilog;
using Serilog.Core;
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
		[Theory, AutoData( additionalTypes: typeof(FormatterFactory) )]
		public void EnsureComposition( CompositionContext context, string text )
		{
			var logger = context.GetExport<ILogger>();
			var actual = ActivationProperties.Factory.Get( logger );
			Assert.Equal( typeof(LoggerFactory), actual );

			Assert.NotSame( DefaultServiceProvider.Instance.Value.Get<ILogger>(), logger );

			var method = GetType().GetMethod( nameof(AnotherMethod), BindingOptions.AllMembers );
			var command = new LogCommand( logger );

			command.Run( new HelloWorld( text, method ) );
			
			var history = context.GetExport<ILoggerHistory>();
			Assert.Same( DefaultServiceProvider.Instance.Value.Get<ILoggerHistory>(), history );
			var message = LogEventMessageFactory.Instance.Create( history.Events ).Last();
			Assert.Contains( text, message );
			
			Assert.Contains( new MethodFormatter( method ).ToString( null, null ), message );
		}

		void AnotherMethod() {}

		class HelloWorld : LoggerTemplate
		{
			public HelloWorld( string text, MethodBase method ) : base( "Hello World! {Text} - {Method}", text, method ) {}
		}

		[Export]
		class LoggerFactory : DragonSpark.Diagnostics.LoggerFactory
		{
			/*public LoggerFactory() : this( new LoggingLevelSwitch() ) {}

			public LoggerFactory( LoggingLevelSwitch logging ) : base( new Factory( logging ).Create ) {}*/
			[ImportingConstructor]
			public LoggerFactory( Func<LoggerConfiguration> configurationSource ) : base( configurationSource ) {}
		}

		[Export]
		class Factory : RecordingLoggerConfigurationFactory
		{
			[ImportingConstructor]
			public Factory( ILoggerHistory history, LoggingLevelSwitch controller ) : base( history, controller, 
				new ICommand<LoggerConfiguration>[] { /*DestructureMethodCommand.Instance,*/ EnrichFromLogContextCommand.Instance }.Select( command => new ConfiguringTransformer<LoggerConfiguration>( command.Run ) ).Fixed()  
				) {}
		}
	}
}