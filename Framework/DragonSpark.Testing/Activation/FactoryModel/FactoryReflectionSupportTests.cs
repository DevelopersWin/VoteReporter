using DragonSpark.Activation.FactoryModel;
using DragonSpark.Composition;
using DragonSpark.Extensions;
using DragonSpark.Testing.Objects;
using System.Composition.Hosting.Core;
using System.Linq;
using Xunit;

namespace DragonSpark.Testing.Activation.FactoryModel
{
	public class FactoryReflectionSupportTests
	{
		[Fact]
		public void GetResultType()
		{
			var expected = typeof(FactoryOfYAC);
			var types = expected.Assembly.DefinedTypes.AsTypes().Where( FactoryTypeFactory.Specification.Instance.IsSatisfiedBy ).Select( FactoryTypeFactory.Instance.Create ).ToArray();
			var type = new DiscoverableFactoryTypeLocator( types ).Create( new CompositionContract( typeof(YetAnotherClass) ) );
			Assert.Equal( expected, type );
		} 
	}
}