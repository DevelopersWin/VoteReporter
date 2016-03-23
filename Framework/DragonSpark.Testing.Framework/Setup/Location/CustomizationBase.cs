using Ploeh.AutoFixture;

namespace DragonSpark.Testing.Framework.Setup.Location
{
	public abstract class CustomizationBase : ICustomization
	{
		public void Customize( IFixture fixture ) => OnCustomize( fixture );

		protected abstract void OnCustomize( IFixture fixture );
	}
}