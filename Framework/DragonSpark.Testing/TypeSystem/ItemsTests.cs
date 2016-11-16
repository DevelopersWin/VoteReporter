using DragonSpark.Diagnostics.Configurations;
using DragonSpark.TypeSystem;
using Xunit;

namespace DragonSpark.Testing.TypeSystem
{
	public class ItemsTests
	{
		[Fact]
		public void DefaultValue()
		{
			var item = Items<ILoggingConfiguration>.Default;
			Assert.NotNull( item );
		}
	}
}