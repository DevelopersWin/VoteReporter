using DragonSpark.Modularity;
using DragonSpark.Testing.Framework;
using DragonSpark.Windows.Modularity;
using Xunit;

namespace DragonSpark.Windows.Testing.Modularity
{
	[Trait( Traits.Category, Traits.Categories.FileSystem )]
	public class ModuleAttributeTests
	{
		[Fact]
		public void StartupLoadedDefaultsToTrue()
		{
			var moduleAttribute = new DynamicModuleAttribute();

			Assert.Equal(false, moduleAttribute.OnDemand);
		}

		[Fact]
		public void CanGetAndSetProperties()
		{
			var moduleAttribute = new DynamicModuleAttribute
			{
				ModuleName = "Test",
				OnDemand = true
			};

			Assert.Equal("Test", moduleAttribute.ModuleName);
			Assert.Equal(true, moduleAttribute.OnDemand);
		}

		[Fact]
		public void ModuleDependencyAttributeStoresModuleName()
		{
			var moduleDependencyAttribute = new ModuleDependencyAttribute("Test");

			Assert.Equal("Test", moduleDependencyAttribute.ModuleName);
		}
	}
}