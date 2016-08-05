using DragonSpark.TypeSystem;
using System.Linq;
using Xunit;

namespace DragonSpark.Testing.TypeSystem
{
	public class PublicPartsTests
	{
		[Fact]
		public void Public()
		{
			var parts = PublicParts.Instance.Get( GetType().Assembly );
			Assert.Single( parts );
			Assert.Equal( "DragonSpark.Testing.Parts.PublicClass", parts.Single().FullName );
		}

		[Fact]
		public void All()
		{
			var parts = AllParts.Instance.Get( GetType().Assembly );
			Assert.Equal( 2, parts.Length );
			var names = parts.Select( type => type.FullName ).ToArray();
			Assert.Contains( "DragonSpark.Testing.Parts.PublicClass", names );
			Assert.Contains( "DragonSpark.Testing.Parts.NonPublicClass", names );
		}
	}
}