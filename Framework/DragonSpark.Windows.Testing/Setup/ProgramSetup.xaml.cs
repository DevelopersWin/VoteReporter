using DragonSpark.Activation;
using DragonSpark.Testing.Framework.Setup;
using System;
using System.Composition;

namespace DragonSpark.Windows.Testing.Setup
{
	[Export]
	public partial class ProgramSetup
	{
		public class AutoDataAttribute : Setup.AutoDataAttribute
		{
			readonly static Func<IServiceProvider, IApplication> ApplicationSource = provider => new Application<ProgramSetup>( provider );

			public AutoDataAttribute() : base( ApplicationSource ) {}
		}

		public ProgramSetup()
		{
			InitializeComponent();
		}
	}
}
