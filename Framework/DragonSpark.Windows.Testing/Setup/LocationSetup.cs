using System.Composition;
using DragonSpark.Testing.Framework.Setup;
using DragonSpark.Testing.Objects;

namespace DragonSpark.Windows.Testing.Setup
{
	[Export]
	public partial class LocationSetup
	{
		public class AutoDataAttribute : Setup.AutoDataAttribute
		{
			public AutoDataAttribute() : base( AssemblyProvider.Instance.Create, serviceProvider => new Application<LocationSetup>( serviceProvider ) ) {}
		}

		public LocationSetup()
		{
			InitializeComponent();
		}
	}
}
