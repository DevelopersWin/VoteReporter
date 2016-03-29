using DragonSpark.Extensions;
using DragonSpark.Testing.Framework;
using DragonSpark.Testing.Objects;
using Microsoft.Practices.Unity;
using Xunit;
using Xunit.Abstractions;

namespace DragonSpark.Windows.Testing.Setup
{
	public class UnitySetupTests : TestBase
	{
		public UnitySetupTests( ITestOutputHelper output ) : base( output )
		{}

		[Theory, UnitySetup.AutoData]
		public void Extension( IUnityContainer sut )
		{
			Assert.NotNull( sut.Configure<TestExtension>() );
		}
		/*[Fact]
		public void Extension()
		{
			using ( var container = CompositionHostFactory.Instance.Create( AssemblyProvider.Instance.Create() ) )
			{
				// new CompositionHostContext().Assign( container );
				var temp = container.GetExport<ConfigureServiceLocationContext>();
			}
			/*var currentMethod = (MethodInfo)MethodBase.GetCurrentMethod();
			using ( new AssignExecutionContextCommand().ExecuteWith( MethodContext.Get( currentMethod ) ) )
			{
				var autoData = new AutoData( FixtureFactory.Instance.Create(), currentMethod );
				using ( new AssignAutoDataCommand().ExecuteWith( autoData ) )
				{
					var instance = new ApplicationWithLocation<UnitySetup>().ExecuteWith( autoData );
					Assert.True( true );
				}
			}#1#

			/*var currentMethod = (MethodInfo)MethodBase.GetCurrentMethod();
			using ( new AssignExecutionContextCommand().ExecuteWith( MethodContext.Get( currentMethod ) ) )
			{
				using ( var container = CompositionHostFactory.Instance.Create( AssemblyProvider.Instance.Create() ) )
				{
					new CompositionHostContext().Assign( container );
					var temp = container.GetExport<ConfigureServiceLocationContext>();
				}
			}#1#
		}*/

		[Theory, UnitySetup.AutoData]
		public void RegisteredName( IUnityContainer sut )
		{
			Assert.NotNull( sut.Resolve<Singleton>( "SomeName" ) );
		}
	}
}