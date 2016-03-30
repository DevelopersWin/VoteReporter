using System.Composition;
using DragonSpark.Testing.Framework.Setup;

namespace DragonSpark.Windows.Testing.Setup
{
	[Export]
	public partial class UnitySetup
	{
		public class AutoDataAttribute : DragonSpark.Testing.Objects.IoC.AutoDataAttribute
		{
			public AutoDataAttribute() : base( serviceProvider => new Application<UnitySetup>( serviceProvider ) ) {}
		}

		public UnitySetup()
		{
			InitializeComponent();
		}
	}
}
