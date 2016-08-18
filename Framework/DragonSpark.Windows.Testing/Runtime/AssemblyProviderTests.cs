using DragonSpark.Extensions;
using DragonSpark.Testing.Framework;
using DragonSpark.TypeSystem;
using DragonSpark.Windows.Runtime;
using System.Linq;
using DragonSpark.Sources;
using Xunit;

namespace DragonSpark.Windows.Testing.Runtime
{
	public class AssemblyProviderTests
	{
		[Theory, Ploeh.AutoFixture.Xunit2.AutoData, Trait( Traits.Category, Traits.Categories.FileSystem )]
		public void Assemblies( FileSystemTypes sut )
		{
			Assert.Same( sut, FileSystemTypes.Instance );
			var assemblies = sut.Get().AsEnumerable().Assemblies();
			var specification = new ApplicationAssemblySpecification( typeof(ISource).Assembly.ToItem() );

			Assert.True( assemblies.All( specification.IsSatisfiedBy ) );
		} 
	}
}