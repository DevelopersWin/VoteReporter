using DragonSpark.Composition;
using DragonSpark.Extensions;
using DragonSpark.Setup;
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
		}

		[Fact]
		public void WithoutConvention()
		{
			var parts = new[] { typeof(IHelloWorld), typeof(HelloWorld) };
			new AssignSystemPartsCommand( parts ).Run();

			var container = new ContainerConfiguration().WithParts( parts ).CreateContainer();
			Assert.Throws<CompositionFailedException>( () => container.GetExport<IHelloWorld>() );
		}

		interface IHelloWorld {}

		class HelloWorld : IHelloWorld {}
	}
}