using DragonSpark.Activation;
using DragonSpark.Runtime.Properties;
using DragonSpark.Testing.Objects;
using DragonSpark.TypeSystem;
using System.Collections.Immutable;
using Xunit;

namespace DragonSpark.Testing.Activation.FactoryModel
{
	public class FactoryReflectionSupportTests
	{
		[Fact]
		public void GetResultType()
		{
			var expected = typeof(FactoryOfYAC);
			var types = FactoryTypeRequests.Instance.GetMany( AssemblyTypes.All.Get( expected.Assembly ).ToImmutableArray() );
			var type = new FactoryTypes( types ).Get( new LocateTypeRequest( typeof(YetAnotherClass) ) );
			Assert.Equal( expected, type );
		} 
	}
}