using DragonSpark.Modularity;
using Xunit;

namespace DragonSpark.Windows.Testing.Modularity
{
	public class ModuleInfoBuilderTests
	{
		[Theory, Ploeh.AutoFixture.Xunit2.AutoData]
		public void Create( ModuleInfoBuilder sut )
		{
			var module = sut.CreateModuleInfo( typeof(Module) );
			Assert.NotNull( module );
		}

		[Module]
		public class Module : IModule
		{
			public bool Initialized { get; private set; }
			
			public void Initialize()
			{
				Initialized = true;
			}
		}
	}
}