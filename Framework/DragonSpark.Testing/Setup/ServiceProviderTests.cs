using DragonSpark.Extensions;
using DragonSpark.Setup;
using DragonSpark.Testing.Objects;
using Xunit;

namespace DragonSpark.Testing.Setup
{
	public class ServiceProviderTests
	{
		[Fact]
		public void BasicTest()
		{
			var target = new Class();

			var composite = new CompositeServiceProvider( new InstanceServiceProvider( target ) );
			var result = composite.Get<Class>();
			Assert.Same( target, result );
		}
	}
}