using DragonSpark.Composition;
using DragonSpark.Testing.Objects.Setup;
using Serilog;
using System.Diagnostics;
using Xunit;

namespace DragonSpark.Testing.Diagnostics
{
	public class RecordingLoggerFactoryTests
	{
		[Theory, DefaultSetup.AutoData]
		public void BasicCompose()
		{
			var temp = Composer.Compose<ILogger>();
			Debugger.Break();
		} 
	}
}