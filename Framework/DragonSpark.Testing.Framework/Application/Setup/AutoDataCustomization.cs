using Ploeh.AutoFixture;
using Ploeh.AutoFixture.AutoMoq;

namespace DragonSpark.Testing.Framework.Application.Setup
{
	public class AutoDataCustomization : CompositeCustomization
	{
		public AutoDataCustomization() : base( ServicesCustomization.Default, new AutoConfiguredMoqCustomization() ) { }
	}

	/*public class AutoDataMoqAttribute : AutoDataAttribute
	{
		public AutoDataMoqAttribute() : base( FixtureFactory<AutoMoqCustomization>.Default.Create ) {}
	}*/

	/*public class SetupFixtureFactory<T> : FixtureFactory<T> where T : SetupCustomization, new() {}*/
}