using DragonSpark.Testing.Framework.Setup;
using System;
using System.Composition;

namespace DragonSpark.Windows.Testing.Setup
{
	[Export]
	public partial class UnitySetup
	{
		public class AutoDataAttribute : DragonSpark.Testing.Objects.IoC.AutoDataAttribute
		{
			readonly static Func<IApplication> ApplicationSource = () => new Application<UnitySetup>();

			public AutoDataAttribute() : base( ApplicationSource ) {}
		}

		public UnitySetup()
		{
			InitializeComponent();
		}
	}
}
