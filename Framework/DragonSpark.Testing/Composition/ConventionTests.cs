using DragonSpark.Composition;
using DragonSpark.Extensions;
using DragonSpark.Setup;
using System.Composition;
using System.Composition.Hosting;
using Xunit;

namespace DragonSpark.Testing.Composition
{
	public class ConventionTests
	{
		[Fact]
		public void Convention()
		{
			var parts = new[] { typeof(IHelloWorld), typeof(HelloWorld) };
			new AssignSystemPartsCommand( parts ).Run();

			var container = new ContainerConfiguration().WithParts( parts, ConventionBuilderFactory.Instance.Get() ).CreateContainer();
			var export = container.GetExport<IHelloWorld>();
			Assert.IsType<HelloWorld>( export );
			Assert.NotSame( export, container.GetExport<IHelloWorld>() );
			Assert.NotSame( container.GetExport<HelloWorld>(), container.GetExport<HelloWorld>() );
			Assert.NotSame( container.GetExport<HelloWorld>(), container.GetExport<IHelloWorld>() );
		}

		[Fact]
		public void WithoutConvention()
		{
			var parts = new[] { typeof(IHelloWorld), typeof(HelloWorld) };
			new AssignSystemPartsCommand( parts ).Run();

			var container = new ContainerConfiguration().WithParts( parts ).CreateContainer();
			Assert.Throws<CompositionFailedException>( () => container.GetExport<IHelloWorld>() );
		}

		[Fact]
		public void Shared()
		{
			var parts = new[] { typeof(IHelloWorldShared), typeof(HelloWorldShared) };
			new AssignSystemPartsCommand( parts ).Run();

			var container = new ContainerConfiguration().WithParts( parts, ConventionBuilderFactory.Instance.Get() ).CreateContainer();
			var export = container.GetExport<IHelloWorldShared>();
			Assert.IsType<HelloWorldShared>( export );
			Assert.Same( export, container.GetExport<IHelloWorldShared>() );
			Assert.Same( container.GetExport<HelloWorldShared>(), container.GetExport<HelloWorldShared>() );
			Assert.Same( container.GetExport<HelloWorldShared>(), container.GetExport<IHelloWorldShared>() );
			
		}

		interface IHelloWorld {}
		class HelloWorld : IHelloWorld {}


		interface IHelloWorldShared {}
		[Shared]
		class HelloWorldShared : IHelloWorldShared {}
	}
}