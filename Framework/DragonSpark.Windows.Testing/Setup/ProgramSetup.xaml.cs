using DragonSpark.Testing.Framework.Setup;
using System;
using System.Composition;

namespace DragonSpark.Windows.Testing.Setup
{
	[Export]
	public partial class ProgramSetup
	{
		public class AutoDataAttribute : DragonSpark.Testing.Framework.IoC.AutoDataAttribute
		{
			readonly static Func<IApplication> ApplicationSource = () => new Application<ProgramSetup>();

			public AutoDataAttribute() : base( ApplicationSource ) {}
		}

		public ProgramSetup()
		{
			InitializeComponent();
		}
	}
}
