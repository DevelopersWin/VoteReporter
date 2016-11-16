using DragonSpark.Windows.Diagnostics;
using Serilog;
using Xunit;

namespace DragonSpark.Windows.Testing.Diagnostics
{
	public class AddRollingFileSinkConfigurationTests
	{
		[Fact]
		public void Coverage()
		{
			var sut = new AddRollingFileSinkConfiguration { PathFormat = "File.txt" };
			sut.FileSizeLimitBytes = sut.FileSizeLimitBytes;
			sut.OutputTemplate = sut.OutputTemplate;
			sut.RetainedFileCountLimit = sut.RetainedFileCountLimit;
			sut.FormatProvider = sut.FormatProvider;
			sut.RestrictedToMinimumLevel = sut.RestrictedToMinimumLevel;
			sut.Get( new LoggerConfiguration() );
		}
	}
}