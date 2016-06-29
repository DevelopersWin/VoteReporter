using DragonSpark.Extensions;
using DragonSpark.Testing.Objects;
using Ploeh.AutoFixture.Xunit2;
using Xunit;

namespace DragonSpark.Testing.Extensions
{
	public class ReflectionExtensionsTests
	{
		[Theory, AutoData]
		void GenericInvoke( Class @class )
		{
			var context = typeof(Static).Adapt().GenericMethods[nameof(Static.Assign)].Make( typeof(Class) );
			context.StaticCall( null );
			
			Assert.Null( Static.Instance );
			
			context.StaticCall( @class );

			Assert.Same( @class, Static.Instance );
		}
	}
}