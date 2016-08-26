using DragonSpark.Activation.Location;
using DragonSpark.Sources.Parameterized;
using System;

namespace DragonSpark.Composition
{
	public class ServiceProviderSource : FixedFactory<IServiceProvider, IServiceProvider>
	{
		public static ServiceProviderSource Default { get; } = new ServiceProviderSource();
		ServiceProviderSource() : base( ServiceProviderFactory.Default.Get, DefaultServiceProvider.Default ) {}
	}
}