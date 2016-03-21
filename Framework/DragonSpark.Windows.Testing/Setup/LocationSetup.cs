using System.Composition;

namespace DragonSpark.Windows.Testing.Setup
{
	[Export]
	public partial class LocationSetup
	{
		public class AutoDataAttribute : DragonSpark.Testing.Objects.Setup.AutoDataAttribute
		{
			public AutoDataAttribute() : base( autoData => new Application<LocationSetup>() ) {}
		}

		public LocationSetup()
		{
			InitializeComponent();
		}
	}
}
