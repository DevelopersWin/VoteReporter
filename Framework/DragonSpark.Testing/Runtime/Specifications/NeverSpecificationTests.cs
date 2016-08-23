using Xunit;

namespace DragonSpark.Testing.Runtime.Specifications
{
	public class NeverSpecificationTests
	{
		[Fact]
		public void Never()
		{
			Assert.False( DragonSpark.Specifications.Specifications.Never.IsSatisfiedBy( null ) );
		} 
	}
}