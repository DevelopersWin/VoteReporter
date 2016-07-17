using DragonSpark.Activation;
using DragonSpark.Configuration;
using DragonSpark.Extensions;
using DragonSpark.Testing.Framework;
using System.Reflection;
using Xunit;
using ExecutionContextStore = DragonSpark.Testing.Framework.ExecutionContextStore;

namespace DragonSpark.Testing.Activation
{
	public class ApplicationContextTests
	{
		[Fact]
		public void Assignment()
		{
			Assert.IsType<ExecutionContextStore>( ExecutionContextLocator.Instance.Value );
			var current = Assert.IsType<ExecutionContext>( Defaults.ExecutionContext() );
			Assert.Same( ExecutionContextStore.Instance.Value, current );
			Assert.Equal( TaskContextStore.Instance.Value, current.Origin );
			Assert.Null( MethodContext.Instance.Value );
			Assert.True( EnableMethodCaching.Instance.Default() );

			var method = MethodBase.GetCurrentMethod();
			using ( var command = new TestingApplicationInitializationCommand( method ).Run( default(object) ) )
			{
				Assert.NotNull( MethodContext.Instance.Value );
				Assert.Same( method, MethodContext.Instance.Value );
				Assert.Same( current, Defaults.ExecutionContext() );
				Assert.False( EnableMethodCaching.Instance.Default() );
			}

			var context = Defaults.ExecutionContext();
			Assert.NotNull( context );
			Assert.NotSame( current, context );

			Assert.Null( MethodContext.Instance.Value );

			Assert.True( EnableMethodCaching.Instance.Default() );
		}
	}
}