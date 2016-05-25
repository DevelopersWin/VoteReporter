using DragonSpark.Testing.Framework;
using Xunit.Abstractions;

namespace DragonSpark.Testing.Runtime
{
	public class TaskFactoryTests : TestCollectionBase
	{
		public TaskFactoryTests( ITestOutputHelper output ) : base( output ) {}

		/*[Fact]
		public void EnsureFlow()
		{
			var currentMethod = MethodBase.GetCurrentMethod();
			object current;
			using ( currentMethod.AsCurrentContext() )
			{
				current = Execution.Current;
				Assert.Same( currentMethod, current );

				object fromTask = null;
				TaskFactory.Instance.Create( () => fromTask = Execution.Current ).Wait();
				Assert.Same( current, fromTask );
			}
			Assert.NotSame( Execution.Current, current );
		}*/
	}
}