using DragonSpark.Application.Setup;
using DragonSpark.Sources.Parameterized;
using System;

namespace DragonSpark.Composition
{
	public sealed class ServiceProviderFactory : AlterationBase<IServiceProvider>
	{
		// [Export( typeof(ITransformer<IServiceProvider>) )]
		public static ServiceProviderFactory Default { get; } = new ServiceProviderFactory();
		ServiceProviderFactory() {}

		public override IServiceProvider Get( IServiceProvider parameter )
		{
			var context = CompositionHostFactory.Default.Get();
			var primary = new ServiceLocator( context );
			var result = new CompositeServiceProvider( new InstanceRepository( context, primary ), primary, parameter );
			return result;
		}
	}
}
