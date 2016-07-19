using DragonSpark.Testing.Framework.Setup;
using System;
using System.Composition;

namespace DragonSpark.Windows.Testing.Setup
{
	[Export]
	public partial class LocationSetup
	{
		public class AutoDataAttribute : DragonSpark.Testing.Framework.IoC.AutoDataAttribute
		{
			public static Func<IApplication> ApplicationSource { get; } = () => new Application<LocationSetup>();

			public AutoDataAttribute() : base( ApplicationSource ) {}
		}

		public LocationSetup()
		{
			InitializeComponent();
		}
	}
}
