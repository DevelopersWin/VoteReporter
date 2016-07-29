using DragonSpark.Activation.IoC;
using DragonSpark.Extensions;
using DragonSpark.Setup.Registration;
using DragonSpark.Testing.Framework;
using DragonSpark.Testing.Framework.Setup;
using DragonSpark.Testing.Objects;
using Microsoft.Practices.Unity;
using Ploeh.AutoFixture.Xunit2;
using System.Composition;
using Xunit;

namespace DragonSpark.Testing.Activation.IoC
{
	[Trait( Traits.Category, Traits.Categories.ServiceLocation ), ContainingTypeAndNested]
	public class RegistrationSupportTests
	{
		[Export]
		public IUnityContainer Container { get; } = UnityContainerFactory.Instance.Create();

		[Theory, Framework.Setup.AutoData]
		public void Mapping( [Frozen]IUnityContainer sut, TransientServiceRegistry registry )
		{
			Assert.Null( sut.TryResolve<IInterface>() );
			registry.Register<IInterface, Class>();

			var first = sut.TryResolve<IInterface>();
			Assert.IsType<Class>( first );

			Assert.NotSame( first, sut.TryResolve<IInterface>() );
		}

		[Theory, Framework.Setup.AutoData]
		public void Persisting( [Frozen]IUnityContainer sut, PersistentServiceRegistry registry )
		{
			Assert.Null( sut.TryResolve<IInterface>() );
			registry.Register<IInterface, Class>();

			var first = sut.TryResolve<IInterface>();
			Assert.IsType<Class>( first );

			Assert.Same( first, sut.TryResolve<IInterface>() );
		}
	}
}