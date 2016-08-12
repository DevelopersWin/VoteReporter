using DragonSpark.Activation;
using DragonSpark.Composition;
using DragonSpark.Extensions;
using DragonSpark.Runtime;
using DragonSpark.Setup;
using System.Composition;
using System.Composition.Hosting;
using System.Linq;
using DragonSpark.Activation.Sources;
using Xunit;

namespace DragonSpark.Testing.Composition
{
	public class SourceExporterTests
	{
		[Fact]
		public void BasicExport()
		{
			var parts = typeof(Source);
			new AssignSystemPartsCommand( parts ).Run();

			var container = new ContainerConfiguration().WithProvider( new SourceExporter() ).WithParts( parts ).CreateContainer();
			var number = container.GetExport<int>();
			Assert.Equal( 6776, number );
		}

		[Fact]
		public void Parameterized()
		{
			var parts = typeof(Source).Append( typeof(ParameterizedSource), typeof(Result) ).ToArray();
			new AssignSystemPartsCommand( parts ).Run();

			var container = new ContainerConfiguration().WithProvider( new SourceExporter() ).WithParts( parts ).CreateContainer();
			var dependency = container.GetExport<Result>();
			Assert.Equal( 6776 + 123, dependency.Number );
		}

		[Fact]
		public void Dependency()
		{
			var parts = typeof(Source).Append( typeof(WithDependency) ).ToArray();
			new AssignSystemPartsCommand( parts ).Run();

			var container = new ContainerConfiguration().WithProvider( new SourceExporter() ).WithParts( parts ).CreateContainer();
			var dependency = container.GetExport<WithDependency>();
			Assert.Equal( 6776, dependency.Number );
		}

		[Fact]
		public void ParameterizedDependency()
		{
			var parts = typeof(Source).Append( typeof(WithDependency) ).ToArray();
			new AssignSystemPartsCommand( parts ).Run();

			var container = new ContainerConfiguration().WithProvider( new SourceExporter() ).WithParts( parts ).CreateContainer();
			var dependency = container.GetExport<WithDependency>();
			Assert.Equal( 6776, dependency.Number );
		}

		[Fact]
		public void Shared()
		{
			var parts = typeof(SharedCounter);
			new AssignSystemPartsCommand( parts ).Run();

			var container = new ContainerConfiguration().WithProvider( new SourceExporter() ).WithParts( parts ).CreateContainer();
			Assert.Equal( 1, container.GetExport<int>() );
			Assert.Equal( 1, container.GetExport<int>() );
		}

		[Fact]
		public void PerRequest()
		{
			var parts = typeof(Counter);
			new AssignSystemPartsCommand( parts ).Run();

			var container = new ContainerConfiguration().WithProvider( new SourceExporter() ).WithParts( parts ).CreateContainer();
			Assert.Equal( 1, container.GetExport<int>() );
			Assert.Equal( 2, container.GetExport<int>() );
		}

		[Fact]
		public void One()
		{
			var parts = typeof(Counter);
			new AssignSystemPartsCommand( parts ).Run();
			var type = ResultTypes.Instance.Get( parts );
			Assert.Equal( typeof(int), type );
		}

		[Fact]
		public void Two()
		{
			var parts = typeof(SharedCounter);
			new AssignSystemPartsCommand( parts ).Run();
			var type = ResultTypes.Instance.Get( parts );
			Assert.Equal( typeof(int), type );
		}

		class Count : Scope<int>
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
		class ParameterizedSource : ParameterizedSourceBase<Source, Result>
		{
			public override Result Get( Source parameter ) => new Result( parameter.Get() + 123 );
		}

		struct Result
		{
			public Result( int number )
			{
				Number = number;
			}

			public int Number { get; }
		}

		[Export]
		class Counter : SourceBase<int>
		{
			public override int Get() => Count.Instance.WithInstance( Count.Instance.Get() + 1 );
		}

		[Export, Shared]
		class SharedCounter : SourceBase<int>
		{
			public override int Get() => Count.Instance.WithInstance( Count.Instance.Get() + 1 );
		}

		[Export]
		class WithDependency
		{
			[ImportingConstructor]
			public WithDependency( int number )
			{
				Number = number;
			}

			public int Number { get; }
		}
	}
}