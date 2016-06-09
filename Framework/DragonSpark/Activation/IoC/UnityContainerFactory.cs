using Microsoft.Practices.Unity;
using PostSharp.Patterns.Contracts;
using System;
using System.Collections.Immutable;

namespace DragonSpark.Activation.IoC
{
	public class UnityContainerFactory : AggregateFactory<IUnityContainer>
	{
		public UnityContainerFactory( [Required] IServiceProvider provider )
			: base( UnityContainerCoreFactory.Instance,
					ImmutableArray.Create<ITransformer<IUnityContainer>>( new ServicesConfigurator( provider ), DefaultUnityExtensions.Instance )
				) {}
	}

	/*public class DefaultUnityContainerFactory : AggregateFactory<IUnityContainer>
	{
		public static DefaultUnityContainerFactory Instance { get; } = new DefaultUnityContainerFactory();

		public DefaultUnityContainerFactory() : base( UnityContainerCoreFactory.Instance.Create, DefaultUnityExtensions.Instance.Create ) {}
	}*/
}