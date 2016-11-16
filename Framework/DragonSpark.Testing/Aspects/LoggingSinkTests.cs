using DragonSpark.Application;
using DragonSpark.Aspects;
using DragonSpark.Commands;
using DragonSpark.Diagnostics;
using DragonSpark.Diagnostics.Configurations;
using DragonSpark.Extensions;
using DragonSpark.Sources;
using DragonSpark.Sources.Scopes;
using DragonSpark.Testing.Framework;
using DragonSpark.TypeSystem;
using JetBrains.Annotations;
using Serilog;
using Serilog.Events;
using System;
using Xunit;
using Xunit.Abstractions;
using LoggerConfigurations = DragonSpark.Diagnostics.LoggerConfigurations;

namespace DragonSpark.Testing.Aspects
{
	public class LoggingSinkTests : TestCollectionBase
	{
		public LoggingSinkTests( ITestOutputHelper output ) : base( output ) {}

		[Theory, Framework.Application.AutoData]
		public void Verify( string message )
		{
			var history = LoggingHistory.Default;
			var template = Templates<Template>.Default.Get( GetType() );
			Assert.NotNull( template );
			Assert.Empty( history.Events );
			template.Execute( message );
			var entry = Assert.Single( history.Events );
			var text = LogEventTextFactory.Default.Get( entry );
			Assert.Contains( message, text );
			Assert.Contains( $"({new TypeFormatter( GetType() ).ToString()})", text );
			Assert.Contains( "Hello world... again! ", text );
		}

		[Fact]
		public void VerifyLogger()
		{
			GetValue();
		}

		static void GetValue()
		{
			
			/*Configurations.Default.Execute();
			var logger = Logger.Default.Get( this );
			;*/
		}

		sealed class Configurations : CompositeCommand
	{
		public static Configurations Default { get; } = new Configurations();

		Configurations( LogEventLevel minimumLevel = LogEventLevel.Verbose ) : base(
			AssignApplicationParts.Default.With( typeof(MethodFormatter), typeof(TypeFormatter), typeof(TypeDefinitionFormatter) ),
			MinimumLevelConfiguration.Default.ToCommand( minimumLevel ),
			LoggerConfigurations.Configure.Instance.WithParameter( DefaultSystemLoggerConfigurations.Default.Append( new AddSeqSinkConfiguration { Endpoint = new Uri( "http://localhost:5341" ), ApiKey = "Gs9MhH4OYUyFNOIiaFIZ" } ).Accept )
		) {}
	}

		[UsedImplicitly]
		sealed class Template : LogCommandBase<string>
		{
			public Template( ILogger logger ) : base( logger, "Hello world... again! {Message}" ) {}
		}
	}
}