using System;
using DragonSpark.Activation.Location;
using DragonSpark.Configuration;
using DragonSpark.Extensions;

namespace DragonSpark.Application.Setup
{
	public sealed class ServiceProviderFactory : ConfigurableFactoryBase<IServiceProvider>
	{
		public static ServiceProviderFactory Default { get; } = new ServiceProviderFactory();
		ServiceProviderFactory() : base( () => DefaultServiceProvider.Default ) {}

		public override IServiceProvider Get() => base.Get().Cached();
	}
}