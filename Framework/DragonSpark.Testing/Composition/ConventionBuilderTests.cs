using System.Composition;
using DragonSpark.Testing.Framework.Setup;
using System.Composition.Convention;
using System.Composition.Hosting;
using System.Reflection;
using DragonSpark.Activation.IoC;
using DragonSpark.Composition;
using DragonSpark.Extensions;
using DragonSpark.Runtime;
using DragonSpark.Runtime.Specifications;
using DragonSpark.Testing.Framework;
using DragonSpark.Testing.Framework.Parameters;
using DragonSpark.Testing.Objects;
using DragonSpark.TypeSystem;
using Microsoft.Practices.ServiceLocation;
using Moq;
using Ploeh.AutoFixture.Xunit2;
using Xunit;
using Type = System.Type;

namespace DragonSpark.Testing.Composition
{
	public class ConventionBuilderTests
	{
		[Theory, Framework.Setup.AutoData]
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

		[RegisterService( typeof(Assembly[]) )]
		[Theory, LocalAutoData( false )]
		public void LocalData( [Service]Type[] sut, Assembly[] assemblies )
		{
			var nested = GetType().Adapt().WithNested();
			Assert.Equal( nested.Length, sut.Length );
			Assert.Equal( nested, sut );

			Assert.Equal( 1, assemblies.Length );
			Assert.Equal( GetType().Assembly, assemblies.Only() );
		}

		[Theory, LocalAutoData]
		public void LocalStrict( [Service]ISingletonLocator sut )
		{
			Assert.IsType<SingletonLocator>( sut );
		}

		class SomeExport {}

		[Shared]
		class SharedExport {}
	}
}
