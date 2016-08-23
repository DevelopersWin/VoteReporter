using DragonSpark.Activation.Location;
using DragonSpark.Configuration;
using DragonSpark.Extensions;
using System;

namespace DragonSpark.Setup
{
	public sealed class ServiceProviderFactory : ConfigurableFactoryBase<IServiceProvider>
	{
		public static ServiceProviderFactory Default { get; } = new ServiceProviderFactory();
		ServiceProviderFactory() : base( () => DefaultServiceProvider.Default ) {}

		public override IServiceProvider Get() => base.Get().Cached();
	}
}