using System.Reflection;
using DragonSpark.Activation.FactoryModel;
using DragonSpark.Testing.Objects;
using Xunit;

namespace DragonSpark.Testing.Activation.FactoryModel
{
	public class FactoryReflectionSupportTests
	{
		[Fact]
		public void GetResultType()
		{
			var type = new FrameworkFactoryTypeLocator( new [] { GetType().Assembly } ).Create( typeof(YetAnotherClass) );
			Assert.Equal( typeof(FactoryOfYAC), type );
		} 
	}
}