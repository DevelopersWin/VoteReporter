using DragonSpark.Diagnostics;
using DragonSpark.Testing.Framework;
using Serilog;
using System.Composition;
using System.Linq;
using DragonSpark.Configuration;
using Serilog.Events;
using Xunit;
using Xunit.Abstractions;

namespace DragonSpark.Testing.Diagnostics
{
	public class CompositionTests : TestCollectionBase
	{
		public CompositionTests( ITestOutputHelper output ) : base( output ) {}

		[Theory, Framework.Setup.AutoData]
		public void BasicCompose( CompositionContext host )
		{
			Initialize();

			var sinkOne = host.GetExport<ILoggerHistory>();
			var sinkTwo = host.GetExport<ILoggerHistory>();
			Assert.Same( sinkOne, sinkTwo );

			var first = host.GetExport<ILogger>();
			var second = host.GetExport<ILogger>();
			Assert.Same( first, second );

			Assert.Single( sinkOne.Events );
			var current = sinkOne.Events.Count();
			first.Information( "Testing this out." );
			Assert.NotEmpty( sinkOne.Events );
			Assert.True( sinkOne.Events.Count() > current );
		}

		static void Initialize() => Configure.Get<DragonSpark.Diagnostics.Configuration>().Profiler.Level = LogEventLevel.Debug;

		[Theory, Framework.Setup.AutoData]
		public void BasicComposeAgain( CompositionContext host )
		{
			Initialize();

			var sinkOne = host.GetExport<ILoggerHistory>();
			var sinkTwo = host.GetExport<ILoggerHistory>();
			Assert.Same( sinkOne, sinkTwo );

			var first = host.GetExport<ILogger>();
			var second = host.GetExport<ILogger>();
			Assert.Same( first, second );

			Assert.Single( sinkOne.Events );
			var current = sinkOne.Events.Count();
			first.Information( "Testing this out." );
			Assert.NotEmpty( sinkOne.Events );
			Assert.True( sinkOne.Events.Count() > current );
		}
	}
}