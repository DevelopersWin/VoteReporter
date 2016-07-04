using DragonSpark.Activation;
using DragonSpark.Composition;
using DragonSpark.Testing.Objects;
using DragonSpark.TypeSystem;
using Xunit;

namespace DragonSpark.Testing.Activation.FactoryModel
{
	public class FactoryReflectionSupportTests
	{
		[Fact]
		public void GetResultType()
		{
			var expected = typeof(FactoryOfYAC);
			var types = FactoryTypeFactory.Instance.CreateMany( AssemblyTypes.All.Create( expected.Assembly ) );
			var type = new FactoryTypeLocator( types ).Get( new LocateTypeRequest( typeof(YetAnotherClass) ) );
			Assert.Equal( expected, type );
		} 
	}
}