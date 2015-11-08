﻿using DragonSpark.Modularity;
using DragonSpark.Setup;

namespace DevelopersWin.VoteReporter.Configuration.Development
{
	public class Module : Module<Setup>
	{
		public Module( IModuleMonitor moduleMonitor, SetupContext context ) : base( moduleMonitor, context )
		{}
	}
}