﻿using System.Composition;

namespace DragonSpark.Testing.Objects.Setup
{
	[Export]
	public partial class DefaultSetup
	{
		public class AutoDataAttribute : Setup.AutoDataAttribute
		{
			public AutoDataAttribute() : base( autoData => new Application<DefaultSetup>( autoData ) ) {}
		}

		public DefaultSetup()
		{
			InitializeComponent();
		}
	}
}
