using DragonSpark.Setup.Registration;
using DragonSpark.Testing.Framework.Setup;
using System.Linq;
using Xunit;
using AssemblyProvider = DragonSpark.Windows.Runtime.AssemblyProvider;

namespace DragonSpark.Windows.Testing.Runtime
{
	public class AssemblyProviderTests
	{
		[Theory, AutoData]
		public void Assemblies( AssemblyProvider sut )
		{
			Assert.NotEqual( sut, AssemblyProvider.Instance );
			Assert.True( sut.Create().All( assembly => assembly.IsDefined( typeof(RegistrationAttribute), false ) ) );
		} 
	}
}