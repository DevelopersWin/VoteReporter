using System;
using System.Composition.Hosting;
using DragonSpark.Composition;
using DragonSpark.Windows.Runtime;

namespace DevelopersWin.VoteReporter.Application.Startup
{
	public class ServiceProviderFactory : DragonSpark.Setup.ApplicationServiceProviderFactory
	{
		public static ServiceProviderFactory Instance { get; } = new ServiceProviderFactory();

		public ServiceProviderFactory() : base( new Func<IServiceProvider>( new DragonSpark.Activation.IoC.ServiceProviderFactory( AssemblyProvider.Instance.Create() ).Create ) ) {}
	}
}
