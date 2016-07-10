﻿using DragonSpark.Windows.Runtime;
using System.Linq;

namespace DevelopersWin.VoteReporter.Application.Startup
{
	public class ServiceProviderFactory : DragonSpark.Setup.ServiceProviderFactory
	{
		public static ServiceProviderFactory Instance { get; } = new ServiceProviderFactory();
		ServiceProviderFactory() : base( new DragonSpark.Activation.IoC.AssemblyBasedServiceProviderFactory( AssemblyProvider.Instance.Create().ToArray() ).Create ) {}
	}
}
