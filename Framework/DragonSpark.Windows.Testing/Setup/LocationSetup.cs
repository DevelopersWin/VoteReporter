using DragonSpark.Activation;
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
			public static Func<IServiceProvider, IApplication> ApplicationSource { get; } = serviceProvider => new Application<LocationSetup>( serviceProvider );

			public AutoDataAttribute() : base( AssemblyProvider.Instance.ToDelegate(), ApplicationSource ) {}
		}

		public LocationSetup()
		{
			InitializeComponent();
		}
	}
}
