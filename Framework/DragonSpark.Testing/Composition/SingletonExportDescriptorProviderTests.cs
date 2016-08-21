using DragonSpark.Composition;
using DragonSpark.Extensions;
using System.Composition;
using System.Composition.Hosting;
using DragonSpark.Sources;
using Xunit;

namespace DragonSpark.Testing.Composition
{
	public class SingletonExportDescriptorProviderTests
	{
		[Fact]
		public void Basic()
		{
			var types = new[] { typeof(Singleton) };
			var container = new ContainerConfiguration().WithParts( types ).WithProvider( new SingletonExportDescriptorProvider( types ) ).CreateContainer();
			var export = container.GetExport<Singleton>();
			Assert.Same( Singleton.Default, export );
		}

		[Fact]
		public void Implementation()
		{
			var types = new[] { typeof(Implemented) };
			var container = new ContainerConfiguration().WithParts( types ).WithProvider( new SingletonExportDescriptorProvider( types ) ).CreateContainer();
			Assert.Same( Implemented.Default, container.GetExport<ISingleton>() );
			Assert.Same( Implemented.Default, container.GetExport<Implemented>() );
		}

		[Fact]
		public void Many()
		{
			var types = new[] { typeof(Implemented), typeof(AnotherImplemented) };
			var container = new ContainerConfiguration().WithParts( types ).WithProvider( new SingletonExportDescriptorProvider( types ) ).CreateContainer();
			var exports = container.GetExports<ISingleton>().Fixed();
			Assert.Contains( Implemented.Default, exports );
			Assert.Contains( AnotherImplemented.Default, exports );
		}

		[Fact]
		public void Source()
		{
			var types = new[] { typeof(Sourced) };
			var container = new ContainerConfiguration().WithParts( types ).WithProvider( new SingletonExportDescriptorProvider( types ) ).CreateContainer();
			Assert.Same( Sourced.Default.Get(), container.GetExport<ISingleton>() );
		}

		class Singleton
		{
			[Export]
			public static Singleton Default { get; } = new Singleton();
			Singleton() {}
		}

		interface ISingleton {}

		class Implemented  : ISingleton
		{
			[Export( typeof(ISingleton) )]
			public static Implemented Default { get; } = new Implemented();
			Implemented() {}
		}

		class AnotherImplemented  : ISingleton
		{
			[Export( typeof(ISingleton) )]
			public static AnotherImplemented Default { get; } = new AnotherImplemented();
			AnotherImplemented() {}
		}

		class Sourced  : ISingleton
		{
			[Export( typeof(ISingleton) )]
			public static ISource<ISingleton> Default { get; } = new FixedSource<ISingleton>( new Sourced() );
			Sourced() {}
		}
	}
}