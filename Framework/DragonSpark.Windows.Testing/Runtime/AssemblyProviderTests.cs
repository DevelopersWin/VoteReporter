using DragonSpark.Setup.Registration;
using System.Linq;
using DragonSpark.Activation;
using DragonSpark.Extensions;
using DragonSpark.TypeSystem;
using Xunit;
using AssemblyProvider = DragonSpark.Windows.Runtime.AssemblyProvider;

namespace DragonSpark.Windows.Testing.Runtime
{
	public class AssemblyProviderTests
	{
		[Theory, Ploeh.AutoFixture.Xunit2.AutoData]
		public void Assemblies( AssemblyProvider sut )
		{
			Assert.NotEqual( sut, AssemblyProvider.Instance );
			var assemblies = sut.Create();
			var specification = new ApplicationAssemblySpecification( typeof(IFactory).Assembly.GetRootNamespace() );

			Assert.True( assemblies.All( specification.IsSatisfiedBy ) );
		} 
	}
}