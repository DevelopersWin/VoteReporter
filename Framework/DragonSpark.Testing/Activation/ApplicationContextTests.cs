using DragonSpark.Activation;
using DragonSpark.Configuration;
using DragonSpark.Extensions;
using DragonSpark.Runtime.Sources;
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
			var before = Execution.Current();
			var current = Assert.IsType<TaskContext>( before );
			Assert.Same( ExecutionContext.Instance.Get(), current );
			Assert.Equal( Identification.Instance.Get(), current.Origin );
			Assert.Null( MethodContext.Instance.Get() );
			Assert.True( EnableMethodCaching.Instance.Get() );

			var method = MethodBase.GetCurrentMethod();
			object inner;
			using ( ApplicationFactory.Instance.Create( method ).Run( new AutoData( FixtureContext.Instance.WithFactory( FixtureFactory<AutoDataCustomization>.Instance.Get ), method ) ) )
			{
				Assert.NotNull( MethodContext.Instance.Get() );
				Assert.Same( method, MethodContext.Instance.Get() );
				inner = Execution.Current();
				Assert.Same( current, inner );
				Assert.False( EnableMethodCaching.Instance.Get() );
			}

			var after = Execution.Current();
			Assert.NotNull( after );
			Assert.NotSame( inner, after );
			Assert.NotSame( current, after );

			Assert.Null( MethodContext.Instance.Get() );

			Assert.True( EnableMethodCaching.Instance.Get() );
		}

		[Export( typeof(ISetup) )]
		class Setup : DragonSpark.Setup.Setup
		{
			public Setup() : base( EnableMethodCaching.Instance.Configured( false ) ) {}
		}
	}
}