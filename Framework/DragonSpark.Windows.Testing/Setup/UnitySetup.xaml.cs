using DragonSpark.Testing.Objects.Setup;

namespace DragonSpark.Windows.Testing.Setup
{
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
