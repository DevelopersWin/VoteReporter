using DragonSpark.Testing.Framework.Setup;
using System;
using System.Composition;
using System.Reflection;

namespace DragonSpark.Windows.Testing.Setup
{
	[Export]
	public partial class UnitySetup
	{
		public class AutoDataAttribute : DragonSpark.Testing.Framework.IoC.AutoDataAttribute
		{
			readonly static Func<MethodBase, IApplication> Source = ApplicationFactory<UnitySetup>.Instance.Create;
			public AutoDataAttribute() : base( Source ) {}
		}

		public UnitySetup()
		{
			InitializeComponent();
		}
	}
}
