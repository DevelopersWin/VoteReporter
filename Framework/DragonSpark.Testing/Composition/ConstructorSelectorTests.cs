using DragonSpark.Composition;
using DragonSpark.Extensions;
using System.Composition;
using System.Composition.Hosting;
using System.Reflection;
using Xunit;

namespace DragonSpark.Testing.Composition
{
	public class ConstructorSelectorTests
	{

		[Fact]
		public void Basic()
		{
			var parts = this.Adapt().WithNested().AsApplicationParts();
			var builder = ConventionBuilderFactory.Default.Get();
			var container = new ContainerConfiguration().WithParts( parts.AsEnumerable(), builder ).CreateContainer();
			var dependency = container.GetExport<Dependency>();
			Assert.NotNull( dependency );

			var primary = Assert.IsType<Primary>( container.GetExport<IPrimary>() );
			Assert.Equal( 2, primary.Selected.GetParameters().Length );

			var exported = container.GetExport<Exported>();
			Assert.Equal( 3, exported.Selected.GetParameters().Length );

		}

		interface IPrimary {}
		class Primary : IPrimary
		{
			public Primary( Dependency dependency, AnotherDependency anotherDependency )
			{
				Selected = MethodBase.GetCurrentMethod();
			}

			public Primary( Dependency dependency, AnotherDependency anotherDependency, NotKnown notKnown )
			{
				Selected = MethodBase.GetCurrentMethod();
			}

			public MethodBase Selected { get; }
		}

		[Export]
		class Exported
		{
			public Exported( Dependency dependency, AnotherDependency anotherDependency )
			{
				Selected = MethodBase.GetCurrentMethod();
			}

			public Exported( Dependency dependency, AnotherDependency anotherDependency, IAnotherDependencyAgain again )
			{
				Selected = MethodBase.GetCurrentMethod();
			}

			public MethodBase Selected { get; }
		}

		interface IDependency {}
		class Dependency : IDependency {}

		interface IAnotherDependency {}
		class AnotherDependency : IAnotherDependency {}

		interface IAnotherDependencyAgain {}
		class AnotherDependencyAgain : IAnotherDependencyAgain {}

		class NotKnown
		{
			
		}
	}
}
