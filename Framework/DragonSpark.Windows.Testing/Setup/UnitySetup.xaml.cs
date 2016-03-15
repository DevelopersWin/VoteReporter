using System.Composition;

namespace DragonSpark.Windows.Testing.Setup
{
	[Export]
	public partial class UnitySetup
	{
		public class AutoDataAttribute : DragonSpark.Testing.Objects.Setup.AutoDataAttribute
		{
			public AutoDataAttribute() : base( () => new Application<UnitySetup>() ) {}
		}

		public UnitySetup()
		{
			InitializeComponent();
		}
	}
}
