using DragonSpark.Activation;
using DragonSpark.Extensions;
using DragonSpark.Runtime.Stores;
using DragonSpark.Testing.Framework;
using DragonSpark.TypeSystem;
using DragonSpark.Windows.Runtime;
using System.Linq;
using Xunit;

namespace DragonSpark.Windows.Testing.Runtime
{
	public class AssemblyProviderTests
	{
		[Theory, Ploeh.AutoFixture.Xunit2.AutoData, Trait( Traits.Category, Traits.Categories.FileSystem )]
		public void Assemblies( FileSystemTypes sut )
		{
			Assert.Same( sut, FileSystemTypes.Instance );
			var assemblies = sut.Get().Assemblies();
			var specification = new ApplicationAssemblySpecification( typeof(IFactory).Assembly.ToItem() );

			Assert.True( assemblies.All( specification.IsSatisfiedBy ) );
		} 
	}
}