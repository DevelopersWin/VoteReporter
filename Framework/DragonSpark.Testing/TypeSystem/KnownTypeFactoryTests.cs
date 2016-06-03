using DragonSpark.Testing.Framework;
using DragonSpark.Testing.Framework.Setup;
using DragonSpark.Testing.Objects;
using DragonSpark.TypeSystem;
using Xunit;

namespace DragonSpark.Testing.TypeSystem
{
	/*[AssemblyProvider.Register]*/
	/*[AssemblyProvider.Types]*/
	[Trait( Traits.Category, Traits.Categories.ServiceLocation )]
	public class KnownTypeFactoryTests
	{
		[Theory, AutoData( false, typeof(Class), typeof(ClassWithProperty), typeof(Derived) )]
		public void Testing( KnownTypeFactory sut )
		{
			var parameter = typeof(Class);

			var items = sut.Create( parameter );

			Assert.NotEmpty( items );

			Assert.All( items, item => Assert.True( parameter.IsAssignableFrom( item ) ) );
		}
	}
}