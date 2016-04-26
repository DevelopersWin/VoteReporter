using DragonSpark.Activation;
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

	public class FixtureFactory<TWith> : FixtureFactory where TWith : ICustomization, new()
	{
		public new static FixtureFactory<TWith> Instance { get; } = new FixtureFactory<TWith>();

		protected override IFixture CreateItem() => base.CreateItem().Customize( new TWith() );
	}

	public class FixtureFactory : FactoryBase<IFixture>
	{
		public static FixtureFactory Instance { get; } = new FixtureFactory();

		protected override IFixture CreateItem() => new Fixture( DefaultEngineParts.Instance );
	}
}