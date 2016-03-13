using System.Linq;
using DragonSpark.Runtime;
using Ploeh.AutoFixture;
using System.Windows.Markup;

namespace DragonSpark.Testing.Framework.Setup
{
	[ContentProperty( nameof(Customizations) )]
	public class CustomizeFixtureCommand : SetupAutoDataCommandBase
	{
		public Collection<ICustomization> PreCustomizations { get; } = new Collection<ICustomization>();

		public Collection<ICustomization> Customizations { get; } = new Collection<ICustomization>();

		public Collection<ICustomization> PostCustomizations { get; } = new Collection<ICustomization>();

		protected override void OnExecute( AutoData parameter ) => parameter.Fixture.Customize( new CompositeCustomization( PreCustomizations.Concat( Customizations ).Concat( PostCustomizations ) ) );
	}
}