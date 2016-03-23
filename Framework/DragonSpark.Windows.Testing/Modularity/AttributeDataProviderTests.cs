using DragonSpark.Extensions;
using DragonSpark.Modularity;
using Xunit;

namespace DragonSpark.Windows.Testing.Modularity
{
	public class AttributeDataProviderTests
	{
		[Theory, Ploeh.AutoFixture.Xunit2.AutoData]
		void GetAll( AttributeDataProvider sut )
		{
			var all = sut.GetAll<string>( typeof(ModuleAttribute), typeof(Module), nameof(ModuleAttribute.ModuleName) );
			Assert.Equal( "SomeModule", all.Only() );
		}

		[Module( ModuleName = "SomeModule" )]
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