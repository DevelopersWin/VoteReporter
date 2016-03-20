using System.Composition;
using DragonSpark.Testing.Framework.Setup;
using System.Composition.Convention;
using System.Composition.Hosting;
using DragonSpark.Composition;
using DragonSpark.Extensions;
using DragonSpark.Runtime.Specifications;
using DragonSpark.Testing.Objects;
using Xunit;

namespace DragonSpark.Testing.Composition
{
	public class ConventionBuilderTests
	{
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

		class SomeExport {}

		[Shared]
		class SharedExport {}
	}
}
