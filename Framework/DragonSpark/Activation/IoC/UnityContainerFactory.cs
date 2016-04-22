using Microsoft.Practices.Unity;
using PostSharp.Patterns.Contracts;
using System;

namespace DragonSpark.Activation.IoC
{
	public class UnityContainerFactory : AggregateFactory<IUnityContainer>
	{
		public UnityContainerFactory( [Required] Func<IServiceProvider> provider )
			: base( UnityContainerCoreFactory.Instance.Create,
					new ServicesConfigurator( provider ).Create,
					DefaultUnityExtensions.Instance.Create
				) {}
	}

	/*public class DefaultUnityContainerFactory : AggregateFactory<IUnityContainer>
	{
		public static DefaultUnityContainerFactory Instance { get; } = new DefaultUnityContainerFactory();

		public DefaultUnityContainerFactory() : base( UnityContainerCoreFactory.Instance.Create, DefaultUnityExtensions.Instance.Create ) {}
	}*/
}