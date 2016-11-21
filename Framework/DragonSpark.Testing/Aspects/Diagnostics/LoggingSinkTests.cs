using DragonSpark.Diagnostics;
using DragonSpark.Testing.Framework;
using DragonSpark.TypeSystem;
using DragonSpark.Windows.Diagnostics;
using JetBrains.Annotations;
using Serilog;
using Serilog.Debugging;
using System;
using Xunit;
using Xunit.Abstractions;

namespace DragonSpark.Testing.Aspects.Diagnostics
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

		/*public sealed class Formatter : IFormatProvider
		{
			public static Formatter Default { get; } = new Formatter();
			Formatter() {}

			public object GetFormat( Type formatType )
			{
				return null;
			}
		}

		[Fact]
		public void VerifyFormatter()
		{
			SelfLog.Enable( s =>
							{
								throw new InvalidOperationException( s );
							} );
			using ( var logger = new AddTraceSinkConfiguration { FormatProvider = Formatter.Default }.Get( new LoggerConfiguration() ).CreateLogger() )
			{
				 var exampleUser = new User { Id = 1, Name = "Adam", Created = DateTime.Now };
		
				logger.Information("Created {@User} on {Created}", exampleUser, DateTime.Now);

			}
		}*/

		[UsedImplicitly]
		sealed class Template : LogCommandBase<string>
		{
			public Template( ILogger logger ) : base( logger, "Hello world... again! {Message}" ) {}
		}
	}

	public class User {
		public int Id { get; set; }

		public string Name { get; set; }

		public DateTime Created { get; set; }
	}
}