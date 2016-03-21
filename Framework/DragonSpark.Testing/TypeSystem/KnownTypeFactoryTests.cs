using DragonSpark.Testing.Framework.Setup;
using DragonSpark.Testing.Objects;
using DragonSpark.TypeSystem;
using Xunit;
using AssemblyProvider = DragonSpark.Testing.Objects.AssemblyProvider;

namespace DragonSpark.Testing.TypeSystem
{
	[AssemblyProvider.Register]
	[AssemblyProvider.Types]
	public class KnownTypeFactoryTests
	{
		[Theory, AutoData]
		public void Testing( KnownTypeFactory sut )
		{
			var parameter = typeof(Class);

			var items = sut.Create( parameter );

			Assert.NotEmpty( items );

			Assert.All( items, type => Assert.True( type.IsSubclassOf( parameter ) ) );
		}
	}
}