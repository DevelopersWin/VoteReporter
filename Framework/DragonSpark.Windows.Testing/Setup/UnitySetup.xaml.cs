﻿using DragonSpark.Testing.Framework.Setup;

namespace DragonSpark.Windows.Testing.Setup
{
	public partial class UnitySetup
	{
		public class AutoDataAttribute : DragonSpark.Testing.Framework.Setup.AutoDataAttribute
		{
			public AutoDataAttribute() : base( FixtureFactory<ApplicationCustomization<UnitySetup>>.Instance.Create ) {}
		}

		public UnitySetup()
		{
			InitializeComponent();
		}
	}
}
