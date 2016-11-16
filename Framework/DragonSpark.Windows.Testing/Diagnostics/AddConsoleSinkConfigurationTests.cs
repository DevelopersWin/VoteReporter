using DragonSpark.Windows.Diagnostics;
using Serilog;
using Xunit;

namespace DragonSpark.Windows.Testing.Diagnostics
{
	public class AddConsoleSinkConfigurationTests
	{
		[Fact]
		public void Coverage()
		{
			var sut = new AddConsoleSinkConfiguration();
			sut.OutputTemplate = sut.OutputTemplate;
			sut.Get( new LoggerConfiguration() );
		}
	}
}