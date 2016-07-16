using DragonSpark.Activation;
using DragonSpark.Extensions;
using DragonSpark.Testing.Framework;
using System.Reflection;
using Xunit;

namespace DragonSpark.Testing.Activation
{
	public class ApplicationContextTests
	{
		[Fact]
		public void Assignment()
		{
			Assert.IsType<object>( Defaults.ExecutionContext() );

			using ( var command = new TestingApplicationInitializationCommand( MethodBase.GetCurrentMethod() ).Run( default(object) ) )
			{
				Assert.IsType<MethodBase>( Defaults.ExecutionContext() );
			}

			Assert.IsType<object>( Defaults.ExecutionContext() );
		}
	}
}