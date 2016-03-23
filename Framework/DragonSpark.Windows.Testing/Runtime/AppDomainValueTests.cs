using DragonSpark.Windows.Runtime;
using Xunit;

namespace DragonSpark.Windows.Testing.Runtime
{
	public class AppDomainValueTests
	{
		[Theory, Ploeh.AutoFixture.Xunit2.AutoData]
		public void Assign( AppDomainValue<int> sut, int number )
		{
			sut.Assign( number );
			Assert.Equal( number, sut.Item );
		}
	}
}