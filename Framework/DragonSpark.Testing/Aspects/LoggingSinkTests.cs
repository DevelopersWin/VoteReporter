using DragonSpark.Diagnostics;
using DragonSpark.Testing.Framework;
using DragonSpark.TypeSystem;
using JetBrains.Annotations;
using Serilog;
using Xunit;
using Xunit.Abstractions;

namespace DragonSpark.Testing.Aspects
{
	public class LoggingSinkTests : TestCollectionBase
	{
		public LoggingSinkTests( ITestOutputHelper output ) : base( output ) {}

		[Theory, Framework.Application.AutoData]
		public void Verify( string message )
		{
			var history = LoggingHistory.Default.Get();
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

		[UsedImplicitly]
		sealed class Template : LogCommandBase<string>
		{
			public Template( ILogger logger ) : base( logger, "Hello world... again! {Message}" ) {}
		}
	}
}