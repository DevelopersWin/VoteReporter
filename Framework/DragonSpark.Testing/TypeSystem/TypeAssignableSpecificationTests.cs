using DragonSpark.Testing.Objects;
using DragonSpark.TypeSystem;
using Xunit;

namespace DragonSpark.Testing.TypeSystem
{
	public class TypeAssignableSpecificationTests
	{
		[Fact]
		public void Test()
		{
			var sut = TypeAssignableSpecification.Default.Get( typeof(IInterface) );

			Assert.True( sut.IsSatisfiedBy( typeof(Class) ) );
			Assert.False( sut.IsSatisfiedBy( GetType() ) );
		} 
	}
}