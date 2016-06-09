using DragonSpark.Activation;
using DragonSpark.Activation.IoC.Specifications;
using DragonSpark.Aspects;
using DragonSpark.Setup;
using DragonSpark.Testing.Framework;
using DragonSpark.Testing.Objects;
using DragonSpark.Testing.Objects.Setup;
using Microsoft.Practices.Unity;
using System;
using Xunit;
using Xunit.Abstractions;

namespace DragonSpark.Testing.Activation.IoC
{
	[Trait( Traits.Category, Traits.Categories.ServiceLocation )]
	[DefaultUnityContainerFactory.Register]
	public class UnityContainerFactoryTests : TestCollectionBase
	{
		public UnityContainerFactoryTests( ITestOutputHelper output ) : base( output ) {}

		[Theory, Framework.Setup.AutoData]
		public void ConstructorSelection( IUnityContainer container )
		{
			var provider = container.Resolve<IServiceProvider>();
			Assert.NotSame( CurrentServiceProvider.Instance.Value, provider );
			Assert.Same( DefaultStoreServiceProvider.Instance, provider );

			Assert.NotNull( container );

			var sut = container.Resolve<ConstructorLocator>();

			// var builder = new BuilderContext( context.BuildPlanStrategies.MakeStrategyChain(), context.Lifetime, context.Policies, new NamedTypeBuildKey<Target>(), null );

			var parameter = new ConstructTypeRequest( typeof(Target), new object() );
			var constructorInfo = sut.Create( parameter );
			Assert.Null( constructorInfo );

			container.RegisterInstance( Output );

			var specification = container.Resolve<RegisteredSpecification>();
			var condition = specification.IsSatisfiedBy( LocatorBase.Coercer.Instance.Coerce( typeof(ITestOutputHelper) ) );
			Assert.True( condition );

			var constructor = sut.Create( new ConstructTypeRequest( typeof(Target), new object() ) );
			Assert.Equal( 1, constructor.GetParameters().Length );

			var resolved = container.Resolve<Target>();
			Assert.NotNull( resolved );
			Assert.Same( Output, resolved.Second() );
			Assert.Same( ClassFactory.Instance.Create(), resolved.First );
		}

		/*[Fact]
		public void Caching()
		{
			var container = DefaultUnityContainerFactory.Instance.Create();
			var first = container.Resolve<Item>();
			var second = container.Resolve<Item>();
			Debugger.Break();
		}

		class Item
		{
			readonly ClassFactory factory;
			public Item( ClassFactory factory )
			{
				this.factory = factory;
			}
		}*/

		class ClassFactory : FactoryBase<Class>
		{
			public static ClassFactory Instance { get; } = new ClassFactory();

			[Freeze]
			public override Class Create() => new Class();
		}

		class Target
		{
			public Target( ITestOutputHelper output ) : this( ClassFactory.Instance.Create(), () => output ) { }

			public Target( IInterface first, Func<ITestOutputHelper> second )
			{
				First = first;
				Second = second;
			}

			public IInterface First { get; }
			public Func<ITestOutputHelper> Second { get; set; }
		}
	}
}