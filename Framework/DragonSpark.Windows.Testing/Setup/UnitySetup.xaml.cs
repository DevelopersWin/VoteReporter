using DragonSpark.Testing.Objects.Setup;
using System.Composition;

namespace DragonSpark.Windows.Testing.Setup
{
	[Export]
	public partial class UnitySetup
	{
		public class AutoDataAttribute : DragonSpark.Testing.Objects.Setup.AutoDataAttribute
		{
			public AutoDataAttribute() : base( () => new ApplicationWithLocation<UnitySetup>() ) {}
		}

		public UnitySetup()
		{
			InitializeComponent();
		}
	}
}
