using System.Linq;
using DragonSpark.Specifications;
using Xunit;

namespace DragonSpark.Testing.Runtime.Specifications
{
	public class AllSpecificationTests
	{
		[Fact]
		public void All()
		{
			var sut = new AllSpecification( new[] { true, true, true }.Select( x => new FixedSpecification( x ) ).ToArray() );
			Assert.True( sut.IsSatisfiedBy() );
		}

		[Fact]
		public void AllNot()
		{
			var sut = new AllSpecification( new[] { true, true, false }.Select( x => new FixedSpecification( x ) ).ToArray() );
			Assert.False( sut.IsSatisfiedBy() );
		}
	}
}