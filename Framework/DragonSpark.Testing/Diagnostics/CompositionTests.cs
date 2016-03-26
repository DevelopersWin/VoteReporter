using DragonSpark.Diagnostics;
using DragonSpark.Testing.Objects.Setup;
using Serilog;
using System.Composition;
using Xunit;

namespace DragonSpark.Testing.Diagnostics
{
	public class CompositionTests
	{
		[Theory, DefaultSetup.AutoData]
		public void BasicCompose( CompositionContext host )
		{
			var sinkOne = host.GetExport<ILoggerHistory>();
			var sinkTwo = host.GetExport<ILoggerHistory>();
			Assert.Same( sinkOne, sinkTwo );

			var first = host.GetExport<ILogger>();
			var second = host.GetExport<ILogger>();
			Assert.Same( first, second );
		}
	}
}