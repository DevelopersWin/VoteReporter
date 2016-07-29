using DragonSpark.Activation;
using DragonSpark.Activation.IoC;
using DragonSpark.Setup.Registration;
using DragonSpark.Testing.Framework;
using Microsoft.Practices.Unity;
using Xunit;
using Xunit.Abstractions;

namespace DragonSpark.Testing.Activation.IoC
{
	public class InjectionFactoryFactoryTests : TestCollectionBase
	{
		public InjectionFactoryFactoryTests( ITestOutputHelper output ) : base( output ) {}

		[LifetimeManager( typeof(ContainerControlledLifetimeManager) )]
		class SingletonMetadataItem {}

		[LifetimeManager( typeof(TransientLifetimeManager) )]
		class TransientMetadataItem {}

		[Fact]
		public void Create()
		{
			var container = UnityContainerFactory.Instance.Create();
			var sut = new InjectionFactoryFactory( typeof(Factory) );
			container.RegisterType<IItem, Item>( new ContainerControlledLifetimeManager() );
			var expected = container.Resolve<IItem>();
			var create = sut.Create( new InjectionMemberParameter( container, typeof(IItem) ) );
			container.RegisterType( typeof(IItem), create );
			Assert.Equal( expected, container.Resolve<IItem>() );
		}

		class Factory : FactoryBase<IItem>
		{
			public override IItem Create() => null;
		}

		interface IItem
		{}

		class Item : IItem
		{}
	}
}