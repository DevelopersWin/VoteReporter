using DragonSpark.Activation;
using DragonSpark.TypeSystem;
using Serilog;
using Xunit;

namespace DragonSpark.Testing.TypeSystem
{
	public class DefaultFactoryTests
	{
		[Fact]
		public void DefaultValue()
		{
			var item = Default<ITransformer<LoggerConfiguration>>.Items;
			Assert.NotNull( item );
		}
	}
}