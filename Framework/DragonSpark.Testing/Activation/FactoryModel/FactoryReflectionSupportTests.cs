using DragonSpark.Activation;
using DragonSpark.Runtime.Properties;
using DragonSpark.Testing.Objects;
using DragonSpark.TypeSystem;
using Xunit;
using FactoryTypeLocator = DragonSpark.Composition.FactoryTypeLocator;

namespace DragonSpark.Testing.Activation.FactoryModel
{
	public class FactoryReflectionSupportTests
	{
		[Fact]
		public void GetResultType()
		{
			var expected = typeof(FactoryOfYAC);
			var types = FactoryTypeLocator.Instance.GetMany( AssemblyTypes.All.Create( expected.Assembly ) );
			var type = new DragonSpark.Activation.FactoryTypeLocator( types ).Get( new LocateTypeRequest( typeof(YetAnotherClass) ) );
			Assert.Equal( expected, type );
		} 
	}
}