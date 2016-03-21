using DragonSpark.Activation.IoC;
using DragonSpark.Windows.Runtime;
using System;
using System.Reflection;

namespace DevelopersWin.VoteReporter.Application.Startup
{
	public class ServiceProviderFactory : DragonSpark.Activation.IoC.ServiceProviderFactory
	{
		public static ServiceProviderFactory Instance { get; } = new ServiceProviderFactory();

		public ServiceProviderFactory() : base( 
			new IntegratedUnityContainerFactory( new Func<Assembly[]>( AssemblyProvider.Instance.Create ) ).Create
		) {}
	}
}
