using DragonSpark.Windows.Diagnostics;
using Serilog;
using Xunit;

namespace DragonSpark.Windows.Testing.Diagnostics
{
	public class AddTraceSinkConfigurationTests
	{
		[Fact]
		public void Coverage()
		{
			var sut = new AddTraceSinkConfiguration();
			sut.OutputTemplate = sut.OutputTemplate;
			sut.Get( new LoggerConfiguration() );
		}
	}
}