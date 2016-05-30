using DragonSpark.Activation;
using DragonSpark.TypeSystem;
using Serilog;
using Xunit;

namespace DragonSpark.Testing.TypeSystem
{
	public class DefaultValueFactoryTests
	{
		[Fact]
		public void DefaultValue()
		{
			var item = Items<ITransformer<LoggerConfiguration>>.Default;
			Assert.NotNull( item );
		}
	}
}