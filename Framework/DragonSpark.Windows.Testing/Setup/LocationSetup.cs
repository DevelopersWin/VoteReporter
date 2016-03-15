using System.Composition;
using DragonSpark.Testing.Objects.Setup;

namespace DragonSpark.Windows.Testing.Setup
{
	[Export]
	public partial class LocationSetup
	{
		public class AutoDataAttribute : DragonSpark.Testing.Objects.Setup.AutoDataAttribute
		{
			public AutoDataAttribute() : base( () => new Application<LocationSetup>() ) {}
		}

		public LocationSetup()
		{
			InitializeComponent();
		}
	}
}
