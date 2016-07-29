using DragonSpark.Activation;
using DragonSpark.Composition;
using DragonSpark.Extensions;
using DragonSpark.Runtime;
using DragonSpark.Runtime.Stores;
using DragonSpark.Setup;
using System.Composition;
using System.Composition.Hosting;
using Xunit;

namespace DragonSpark.Testing.Composition
{
	public class SourceExporterTests
	{
		[Fact]
		public void BasicExport()
		{
			var parts = typeof(Source);
			new ApplySystemPartsConfiguration( parts ).Run();

			var container = new ContainerConfiguration().WithProvider( new SourceExporter() ).WithParts( parts ).CreateContainer();
			var number = container.GetExport<int>();
			Assert.Equal( 6776, number );
		}

		[Fact]
		public void Shared()
		{
			var parts = typeof(SharedCounter);
			new ApplySystemPartsConfiguration( parts ).Run();

			var container = new ContainerConfiguration().WithProvider( new SourceExporter() ).WithParts( parts ).CreateContainer();
			Assert.Equal( 1, container.GetExport<int>() );
			Assert.Equal( 1, container.GetExport<int>() );
		}

		[Fact]
		public void PerRequest()
		{
			var parts = typeof(Counter);
			new ApplySystemPartsConfiguration( parts ).Run();

			var container = new ContainerConfiguration().WithProvider( new SourceExporter() ).WithParts( parts ).CreateContainer();
			Assert.Equal( 1, container.GetExport<int>() );
			Assert.Equal( 2, container.GetExport<int>() );
		}

		class Count : Configuration<int>
		{
			public static Count Instance { get; } = new Count();
			Count() {}
		}

		[Export]
		class Source : SourceBase<int>
		{
			public override int Get() => 6776;
		}

		[Export]
		class Counter : SourceBase<int>
		{
			public override int Get() => Count.Instance.Assigned( Count.Instance.Value + 1 );
		}

		[Export, Shared]
		class SharedCounter : SourceBase<int>
		{
			public override int Get() => Count.Instance.Assigned( Count.Instance.Value + 1 );
		}
	}
}