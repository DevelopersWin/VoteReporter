using Microsoft.Practices.Unity;
using PostSharp.Patterns.Contracts;
using System;

namespace DragonSpark.Activation.IoC
{
	public class UnityContainerFactory : AggregateFactory<IUnityContainer>
	{
		public UnityContainerFactory( [Required] IServiceProvider provider )
			: base( UnityContainerCoreFactory.Instance.ToDelegate(), new ServicesConfigurator( provider ).ToDelegate(), DefaultUnityExtensions.Instance.ToDelegate() ) {}
	}
}