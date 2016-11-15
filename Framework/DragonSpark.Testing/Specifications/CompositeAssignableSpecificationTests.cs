using DragonSpark.Specifications;
using Xunit;

namespace DragonSpark.Testing.Specifications
{
	public class CompositeAssignableSpecificationTests
	{
		[Fact]
		public void Coverage()
		{
			Assert.True( new CompositeAssignableSpecification( GetType() ).IsSatisfiedBy( GetType() ) );
			// Assert.True( new AdapterAssignableSpecification( GetType().Adapt() ).IsSatisfiedBy( GetType() ) );
			Assert.False( new CompositeAssignableSpecification( GetType() ).IsSatisfiedBy( typeof(string) ) );
			//Assert.False( new AdapterAssignableSpecification( GetType().Adapt() ).IsSatisfiedBy( typeof(string) ) );
		}
	}
}