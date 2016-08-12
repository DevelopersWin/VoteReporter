using DragonSpark.Sources;
using Ploeh.AutoFixture;
using Ploeh.AutoFixture.AutoMoq;

namespace DragonSpark.Testing.Framework.Setup
{
	public class AutoDataCustomization : CompositeCustomization
	{
		public AutoDataCustomization() : base( ServicesCustomization.Instance, new AutoConfiguredMoqCustomization() ) { }
	}

	/*public class AutoDataMoqAttribute : AutoDataAttribute
	{
		public AutoDataMoqAttribute() : base( FixtureFactory<AutoMoqCustomization>.Instance.Create ) {}
	}*/

	/*public class SetupFixtureFactory<T> : FixtureFactory<T> where T : SetupCustomization, new() {}*/

	public class FixtureFactory<TWith> : FixtureFactoryBase where TWith : ICustomization, new()
	{
		public static FixtureFactory<TWith> Instance { get; } = new FixtureFactory<TWith>();
		FixtureFactory() {}

		public override IFixture Get() => base.Get().Customize( new TWith() );
	}

	public abstract class FixtureFactoryBase : SourceBase<IFixture>
	{
		// public static FixtureFactory Instance { get; } = new FixtureFactory();

		public override IFixture Get() => new Fixture( DefaultEngineParts.Instance );
	}
}