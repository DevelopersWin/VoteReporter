using DragonSpark.Testing.Framework;
using Microsoft.Practices.Unity;
using Xunit;
using LifetimeManagerFactory = DragonSpark.Activation.IoC.LifetimeManagerFactory;

namespace DragonSpark.Testing.Activation.IoC
{
	[Trait( Traits.Category, Traits.Categories.ServiceLocation )]
	public class LifetimeManagerFactoryTests
	{
		[Map( typeof(IUnityContainer), typeof(UnityContainer) )]
		[Theory, Framework.Setup.AutoData]
		public void LocateOnFactory( LifetimeManagerFactory sut )
		{
			var type = sut.Create( typeof(LifetimeManagerFactory) );
			Assert.NotNull( type );
			Assert.IsType<ContainerControlledLifetimeManager>( type );

			// Assert.IsType<ContainerControlledLifetimeManager>( sut.Create( typeof(TryContext) ) );

			// Assert.IsType<ContainerControlledLifetimeManager>( sut.Create( typeof(TryContextElevated) ) );
		} 
	}
}