﻿using DragonSpark.Extensions;
using PostSharp.Aspects;

namespace DragonSpark.Testing.Parts.Development
{
	public partial class Initialize
	{
		[ModuleInitializer( 0 ), Framework.Runtime]
		public static void Execute() => new Initialize().Run();

		Initialize()
		{
			InitializeComponent();
		}
	}
}
