using Microsoft.Practices.Unity;
using PostSharp.Patterns.Contracts;
using System;

namespace DragonSpark.Activation.IoC
{
	public class UnityContainerFactory : AggregateFactory<IUnityContainer>
	{
		readonly static Func<IUnityContainer> Primary = UnityContainerCoreFactory.Instance.Create;
		readonly static Func<IUnityContainer, IUnityContainer> Default = DefaultUnityExtensions.Instance.Create;

		public UnityContainerFactory( [Required] IServiceProvider provider )
			: base( Primary, new ServicesConfigurator( provider ).ToDelegate(), Default ) {}
	}
}