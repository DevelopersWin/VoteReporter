using DragonSpark.Sources.Parameterized;
using DragonSpark.TypeSystem;
using Serilog;
using Xunit;

namespace DragonSpark.Testing.TypeSystem
{
	public class InverseCacheValueFactoryTests
	{
		[Fact]
		public void DefaultValue()
		{
			var item = Items<IAlteration<LoggerConfiguration>>.Default;
			Assert.NotNull( item );
		}
	}
}