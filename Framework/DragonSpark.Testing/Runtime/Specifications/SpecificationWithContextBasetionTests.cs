using DragonSpark.Runtime.Specifications;
using Ploeh.AutoFixture.Xunit2;
using Xunit;

namespace DragonSpark.Testing.Runtime.Specifications
{
	public class SpecificationWithContextBasetionTests
	{
		[Theory, AutoData]
		public void Equal( [Frozen]object item, EqualityContextAwareSpecification sut )
		{
			Assert.True( sut.IsSatisfiedBy( item ) );
			Assert.False( sut.IsSatisfiedBy( new object() ) );
		} 
	}
}