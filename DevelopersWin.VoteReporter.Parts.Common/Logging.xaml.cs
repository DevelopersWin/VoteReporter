﻿using DragonSpark.Sources.Parameterized;
using Serilog;
using System.Composition;

namespace DevelopersWin.VoteReporter.Parts.Common
{
	[Export( typeof(IAlteration<LoggerConfiguration>) ), Shared]
	public partial class Logging
	{
		public Logging()
		{
			InitializeComponent();
		}
	}
}
