using DragonSpark.Activation;
using DragonSpark.Configuration;
using DragonSpark.Extensions;
using DragonSpark.Runtime.Stores;
using DragonSpark.Setup;
using DragonSpark.Testing.Framework;
using DragonSpark.Testing.Framework.Setup;
using System.Composition;
using System.Reflection;
using Xunit;
using ExecutionContext = DragonSpark.Testing.Framework.ExecutionContext;

namespace DragonSpark.Testing.Activation
{
	[ContainingTypeAndNested]
	public class ApplicationContextTests
	{
		[Fact]
		public void Assignment()
		{
			var current = Assert.IsType<TaskContext>( Execution.Current() );
			Assert.Same( ExecutionContext.Instance.Get(), current );
			Assert.Equal( IdentificationStore.Instance.Value, current.Origin );
			Assert.Null( MethodContext.Instance.Get() );
			Assert.True( EnableMethodCaching.Instance.Get() );

			var method = MethodBase.GetCurrentMethod();
			using ( ApplicationFactory.Instance.Create( method ).Run( new AutoData( FixtureContext.Instance.Assigned( FixtureFactory<AutoDataCustomization>.Instance.Create() ), method ) ) )
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

		[Export( typeof(ISetup) )]
		class Setup : DragonSpark.Setup.Setup
		{
			public Setup() : base( EnableMethodCaching.Instance.From( false ) ) {}
		}
	}
}