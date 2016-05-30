using DragonSpark.Activation;
using Xunit;

namespace DragonSpark.Testing.Activation
{
	public class IExecutionContextTests
	{
		[Fact]
		public void Item()
		{
			Assert.Equal( typeof(ExecutionContext), ExecutionContext.Instance.Value.GetType() );
			Assert.Equal( ExecutionContext.Instance, ExecutionContext.Instance.Value );
		} 
	}
}