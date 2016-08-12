using DragonSpark.Composition;
using DragonSpark.Extensions;
using System.Composition;
using System.Composition.Hosting;
using DragonSpark.Activation.Sources;
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
			Assert.Same( Singleton.Instance, export );
		}

		[Fact]
		public void Implementation()
		{
			var types = new[] { typeof(Implemented) };
			var container = new ContainerConfiguration().WithParts( types ).WithProvider( new SingletonExportDescriptorProvider( types ) ).CreateContainer();
			Assert.Same( Implemented.Instance, container.GetExport<ISingleton>() );
			Assert.Same( Implemented.Instance, container.GetExport<Implemented>() );
		}

		[Fact]
		public void Many()
		{
			var types = new[] { typeof(Implemented), typeof(AnotherImplemented) };
			var container = new ContainerConfiguration().WithParts( types ).WithProvider( new SingletonExportDescriptorProvider( types ) ).CreateContainer();
			var exports = container.GetExports<ISingleton>().Fixed();
			Assert.Contains( Implemented.Instance, exports );
			Assert.Contains( AnotherImplemented.Instance, exports );
		}

		[Fact]
		public void Source()
		{
			var types = new[] { typeof(Sourced) };
			var container = new ContainerConfiguration().WithParts( types ).WithProvider( new SingletonExportDescriptorProvider( types ) ).CreateContainer();
			Assert.Same( Sourced.Instance.Get(), container.GetExport<ISingleton>() );
		}

		class Singleton
		{
			[Export]
			public static Singleton Instance { get; } = new Singleton();
			Singleton() {}
		}

		interface ISingleton {}

		class Implemented  : ISingleton
		{
			[Export( typeof(ISingleton) )]
			public static Implemented Instance { get; } = new Implemented();
			Implemented() {}
		}

		class AnotherImplemented  : ISingleton
		{
			[Export( typeof(ISingleton) )]
			public static AnotherImplemented Instance { get; } = new AnotherImplemented();
			AnotherImplemented() {}
		}

		class Sourced  : ISingleton
		{
			[Export( typeof(ISingleton) )]
			public static ISource<ISingleton> Instance { get; } = new FixedSource<ISingleton>( new Sourced() );
			Sourced() {}
		}
	}
}