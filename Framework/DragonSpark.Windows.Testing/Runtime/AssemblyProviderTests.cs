using DragonSpark.Activation;
using DragonSpark.Extensions;
using DragonSpark.Testing.Framework;
using DragonSpark.TypeSystem;
using System.Linq;
using Xunit;
using AssemblyProvider = DragonSpark.Windows.Runtime.AssemblyProvider;

namespace DragonSpark.Windows.Testing.Runtime
{
	public class AssemblyProviderTests
	{
		[Theory, Ploeh.AutoFixture.Xunit2.AutoData, Trait( Traits.Category, Traits.Categories.FileSystem )]
		public void Assemblies( AssemblyProvider sut )
		{
			Assert.Same( sut, AssemblyProvider.Instance );
			var assemblies = sut.Get().Assemblies();
			var specification = new ApplicationAssemblySpecification( typeof(IFactory).Assembly.ToItem() );

			Assert.True( assemblies.All( specification.IsSatisfiedBy ) );
		} 
	}
}