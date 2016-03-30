using System.Composition;
using DragonSpark.Testing.Framework.Setup;

namespace DragonSpark.Windows.Testing.Setup
{
	[Export]
	public partial class LocationSetup
	{
		public class AutoDataAttribute : Setup.AutoDataAttribute
		{
			public AutoDataAttribute() : base( serviceProvider => new Application<LocationSetup>( serviceProvider ) ) {}
		}

		public LocationSetup()
		{
			InitializeComponent();
		}
	}
}
