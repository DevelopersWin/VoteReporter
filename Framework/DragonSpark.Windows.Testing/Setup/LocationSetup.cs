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
			readonly static DelegatedFactory<IServiceProvider, IApplication> ApplicationSource = new DelegatedFactory<IServiceProvider, IApplication>( serviceProvider => new Application<LocationSetup>( serviceProvider ) );

			public AutoDataAttribute() : base( AssemblyProvider.Instance, ApplicationSource ) {}
		}

		public LocationSetup()
		{
			InitializeComponent();
		}
	}
}
