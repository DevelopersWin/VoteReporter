using DragonSpark.Activation.IoC;
using DragonSpark.Composition;
using DragonSpark.Extensions;
using DragonSpark.Runtime.Specifications;
using DragonSpark.Testing.Framework;
using DragonSpark.Testing.Framework.Parameters;
using DragonSpark.Testing.Framework.Setup;
using DragonSpark.Testing.Objects;
using System.Composition;
using System.Composition.Convention;
using System.Composition.Hosting;
using System.Linq;
using System.Reflection;
using Xunit;
using Xunit.Abstractions;
using SingletonLocator = DragonSpark.Activation.IoC.SingletonLocator;
using Type = System.Type;

namespace DragonSpark.Testing.Composition
{
	public class ConventionBuilderTests : TestBase
	{
		public ConventionBuilderTests( ITestOutputHelper output ) : base( output ) {}

		[Theory, AutoData]
		public void BasicConvention( ContainerConfiguration configuration, ConventionBuilder sut )
		{
			sut.ForTypesMatching( AlwaysSpecification.Instance.IsSatisfiedBy ).Export();
			var types = this.Adapt().WithNested();
			var container = configuration.WithParts( types, sut ).CreateContainer();
			var export = container.GetExport<SomeExport>();
			Assert.NotNull( export );
			Assert.NotSame( export, container.GetExport<SomeExport>() );

			var invalid = container.TryGet<Item>();
			Assert.Null( invalid );

			var shared = container.GetExport<SharedExport>();
			Assert.NotNull( shared );
			Assert.Same( shared, container.GetExport<SharedExport>() );
		}

		// [RegisterService( typeof(Assembly[]) )]

		[Theory, AutoData( false )]
		public void LocalData( [Service]Type[] sut, [Service]Assembly[] assemblies )
		{
			var items = sut./*Except( FrameworkTypes.Instance.Create() ).*/Fixed();

			var nested = GetType().Adapt().WithNested();
			Assert.Equal( nested.Length, items.Length );
			Assert.Equal( nested, items );

			Assert.Empty( assemblies );

			/*Assert.Equal( 1, assemblies.Length );
			Assert.Equal( GetType().Assembly, assemblies.Only() );*/
		}

		[Theory, AutoData]
		public void LocalStrict( ISingletonLocator sut )
		{
			Assert.IsType<SingletonLocator>( sut );
		}

		class SomeExport {}

		[Shared]
		class SharedExport {}
	}
}
