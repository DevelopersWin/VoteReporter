using DragonSpark.Testing.Framework.Setup;
using DragonSpark.Testing.Objects;
using System;
using System.Composition;

namespace DragonSpark.Windows.Testing.Setup
{
	[Export]
	public partial class LocationSetup
	{
		public class AutoDataAttribute : Setup.AutoDataAttribute
		{
			public static Func<IApplication> ApplicationSource { get; } = () => new Application<LocationSetup>();

			public AutoDataAttribute() : base( AssemblyProvider.Instance.Create(), ApplicationSource ) {}
		}

		public LocationSetup()
		{
			InitializeComponent();
		}
	}
}
