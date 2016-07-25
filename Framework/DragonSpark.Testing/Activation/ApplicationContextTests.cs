using DragonSpark.Activation;
using DragonSpark.Configuration;
using DragonSpark.Extensions;
using DragonSpark.Runtime.Stores;
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
			// Assert.IsType<ExecutionContextStore>( ExecutionContextLocator.Instance.Value );
			var current = Assert.IsType<ExecutionContext>( Execution.Current() );
			Assert.Same( ExecutionContextStore.Instance.Value, current );
			Assert.Equal( TaskContextStore.Instance.Value, current.Origin );
			Assert.Null( MethodContext.Instance.Get() );
			Assert.True( EnableMethodCaching.Instance.Get() );

			var method = MethodBase.GetCurrentMethod();
			using ( var command = new TestingApplicationInitializationCommand().Run( default(object) ) )
			{
				Assert.NotNull( MethodContext.Instance.Get() );
				Assert.Same( method, MethodContext.Instance.Get() );
				Assert.Same( current, Execution.Current() );
				Assert.False( EnableMethodCaching.Instance.Get() );
			}

			var context = Execution.Current();
			Assert.NotNull( context );
			Assert.NotSame( current, context );

			Assert.Null( MethodContext.Instance.Get() );

			Assert.True( EnableMethodCaching.Instance.Get() );
		}
	}
}