using DragonSpark.Activation;
using DragonSpark.Composition;
using DragonSpark.Extensions;
using DragonSpark.Testing.Objects;
using Xunit;

namespace DragonSpark.Testing.Activation.FactoryModel
{
	public class FactoryReflectionSupportTests
	{
		[Fact]
		public void GetResultType()
		{
			var expected = typeof(FactoryOfYAC);
			var types = FactoryTypeFactory.Instance.CreateMany( expected.Assembly.DefinedTypes.AsTypes() );
			var type = new FactoryTypeLocator( types ).Create( new LocateTypeRequest( typeof(YetAnotherClass) ) );
			Assert.Equal( expected, type );
		} 
	}
}