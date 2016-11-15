using DragonSpark.Specifications;
using Xunit;

namespace DragonSpark.Testing.Specifications
{
	public class CompositeInstanceSpecificationTests
	{
		[Fact]
		public void Coverage()
		{
			var sut = new CompositeInstanceSpecification( GetType() );
			Assert.True( sut.IsSatisfiedBy( this ) );
			Assert.False( sut.IsSatisfiedBy( GetType() ) );
			
			Assert.False( sut.IsSatisfiedBy( "Hello World!" ) );
		}
	}
}