using DragonSpark.Modularity;
using DragonSpark.Testing.Framework;
using DragonSpark.Windows.Modularity;
using Xunit;
using ModuleInfoBuilder = DragonSpark.Windows.Modularity.ModuleInfoBuilder;

namespace DragonSpark.Windows.Testing.Modularity
{
	[Trait( Traits.Category, Traits.Categories.Modularity )]
	public class DirectoryModuleInfoProviderTests
	{
		[Theory, Ploeh.AutoFixture.Xunit2.AutoData]
		public void Cover()
		{
			using ( new DirectoryModuleInfoProvider( ModuleInfoBuilder.Instance, new[] { "Notexists.dll" }, "." ) )
			{
				/*var assembly = Assembly.ReflectionOnlyLoad( "xunit.runner.visualstudio.testadapter, Version=2.1.0.1129, Culture=neutral, PublicKeyToken=null" );
				Assert.Null( assembly );*/
			}
		}

		[Theory, Ploeh.AutoFixture.Xunit2.AutoData]
		public void NotExist( RemoteModuleInfoProviderFactory sut )
		{
			using ( var provider = sut.Create( new LoadRemoteModuleInfoParameter( new[] { typeof( IModule ).Assembly.Location }, DirectoryModuleCatalogTests.InvalidModulesDirectory ) ) )
			{
				Assert.Empty( provider.GetModuleInfos() );
			}
		}
	}
}