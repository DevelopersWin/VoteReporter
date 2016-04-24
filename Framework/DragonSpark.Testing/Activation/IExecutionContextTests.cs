using DragonSpark.Activation;
using Xunit;

namespace DragonSpark.Testing.Activation
{
	public class IExecutionContextTests
	{
		[Fact]
		public void Item()
		{
			Assert.Equal( typeof(string), ExecutionContext.Instance.Value.GetType() );
			Assert.Equal( "DefaultExecutionContext", ExecutionContext.Instance.Value );
		} 
	}
}