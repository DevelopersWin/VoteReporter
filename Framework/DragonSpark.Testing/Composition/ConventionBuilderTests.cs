using DragonSpark.Activation;
using DragonSpark.Composition;
using DragonSpark.Extensions;
using DragonSpark.Runtime.Specifications;
using DragonSpark.Testing.Framework;
using DragonSpark.Testing.Framework.Setup;
using DragonSpark.Testing.Objects;
using System;
using System.Collections.Immutable;
using System.Composition;
using System.Composition.Convention;
using System.Composition.Hosting;
using System.Linq;
using System.Reflection;
using Xunit;
using Xunit.Abstractions;

namespace DragonSpark.Testing.Composition
{
	[Trait( Traits.Category, Traits.Categories.ServiceLocation )]
	public class ConventionBuilderTests : TestCollectionBase
	{
		public ConventionBuilderTests( ITestOutputHelper output ) : base( output ) {}

		[Theory, AutoData]
		public void BasicConvention( ContainerConfiguration configuration, ConventionBuilder sut )
		{
			sut.ForTypesMatching( Specifications.Always.IsSatisfiedBy ).Export();
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

		[Theory, AutoData, ContainingTypeAndNested]
		public void LocalData( ImmutableArray<Type> sut, ImmutableArray<Assembly> assemblies )
		{
			var items = sut.Fixed();

			var nested = GetType().Adapt().WithNested();
			Assert.Equal( nested.Length, items.Length );
			Assert.Equal( nested.OrderBy( type => type.FullName ), items.OrderBy( type => type.FullName ) );

			Assert.Single( assemblies );
			Assert.Equal( GetType().Assembly, assemblies.Only() );
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
