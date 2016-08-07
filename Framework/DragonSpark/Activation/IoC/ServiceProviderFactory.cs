using DragonSpark.Runtime;
using DragonSpark.Setup;
using System;
using System.Composition;

namespace DragonSpark.Activation.IoC
{
	public sealed class ServiceProviderFactory : TransformerBase<IServiceProvider>
	{
		[Export( typeof(ITransformer<IServiceProvider>) )]
		public static ServiceProviderFactory Instance { get; } = new ServiceProviderFactory();
		ServiceProviderFactory() {}

		public override IServiceProvider Get( IServiceProvider parameter )
		{
			var primary = new ServiceLocator( UnityContainerFactory.Instance.Create() );
			var result = new CompositeServiceProvider( new InstanceServiceProvider( primary ), primary, parameter );
			return result;
		}
	}

	[Export( typeof(ISetup) )]
	public class InitializeLocationCommand : InitializeServiceProviderCommandBase
	{
		// public static ISetup Instance { get; } = new InitializeLocationCommand();
		public InitializeLocationCommand() : base( ServiceCoercer<ServiceLocator>.Instance.Coerce )
		{
			Priority = Priority.AfterHigh;
		}
	}
}