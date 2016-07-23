using DragonSpark.Testing.Framework.Setup;
using System;
using System.Composition;
using System.Reflection;

namespace DragonSpark.Windows.Testing.Setup
{
	[Export]
	public partial class LocationSetup
	{
		public class AutoDataAttribute : DragonSpark.Testing.Framework.IoC.AutoDataAttribute
		{
			readonly static Func<MethodBase, IApplication> Source = ApplicationFactory<LocationSetup>.Instance.Create;
			public AutoDataAttribute() : base( Source ) {}
		}

		public LocationSetup()
		{
			InitializeComponent();
		}
	}
}
