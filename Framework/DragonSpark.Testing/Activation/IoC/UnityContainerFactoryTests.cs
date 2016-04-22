using DragonSpark.Activation;
using DragonSpark.Activation.IoC;
using DragonSpark.Aspects;
using DragonSpark.Extensions;
using DragonSpark.Setup;
using DragonSpark.Testing.Framework;
using DragonSpark.Testing.Objects;
using DragonSpark.Testing.Objects.Setup;
using Microsoft.Practices.ObjectBuilder2;
using Microsoft.Practices.Unity;
using System;
using Xunit;
using Xunit.Abstractions;

namespace DragonSpark.Testing.Activation.IoC
{
	[DefaultUnityContainerFactory.Register]
	public class UnityContainerFactoryTests : TestCollectionBase
	{
		public UnityContainerFactoryTests( ITestOutputHelper output ) : base( output ) {}

		[Theory, Framework.Setup.AutoData]
		public void ConstructorSelection( IUnityContainer container )
		{
			var provider = container.Resolve<IServiceProvider>();
			Assert.NotSame( CurrentServiceProvider.Instance.Item, provider );
			Assert.Same( DefaultServiceProvider.Instance.Item, provider );

			Assert.NotNull( container );

			var sut = new ConstructorSelectorPolicy( () => container.Resolve<ResolvableTypeSpecification>() );

			var context = container.Resolve<ExtensionContext>();
			var policyList = container.Resolve<IPolicyList>();

			var builder = new BuilderContext( context.BuildPlanStrategies.MakeStrategyChain(), context.Lifetime, context.Policies, new NamedTypeBuildKey<Target>(), null );

			var first = sut.SelectConstructor( builder, policyList );
			Assert.Equal( 2, first.Constructor.GetParameters().Length );

			container.RegisterInstance( Item );
			var constructor = sut.SelectConstructor( builder, policyList );
			Assert.Equal( 1, constructor.Constructor.GetParameters().Length );

			var resolved = container.Resolve<Target>();
			Assert.NotNull( resolved );
			Assert.Same( Item, resolved.Second() );
			Assert.Same( ClassFactory.Instance.Create(), resolved.First );
		}

		class ClassFactory : FactoryBase<Class>
		{
			public static ClassFactory Instance { get; } = new ClassFactory();

			[Freeze]
			protected override Class CreateItem() => new Class();
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