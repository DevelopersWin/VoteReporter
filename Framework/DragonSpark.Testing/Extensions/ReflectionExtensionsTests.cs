using DragonSpark.Testing.Objects;
using DragonSpark.TypeSystem;
using DragonSpark.TypeSystem.Generics;
using Ploeh.AutoFixture.Xunit2;
using Xunit;

namespace DragonSpark.Testing.Extensions
{
	public class ReflectionExtensionsTests
	{
		[Theory, AutoData]
		void GenericInvoke( Class @class )
		{
			var context = typeof(Static).GetCommand( nameof(Static.Assign) ).Make( typeof(Class) );
			context.Invoke( new object[] { null } );
			
			Assert.Null( Static.Instance );
			
			context.Invoke( @class );

			Assert.Same( @class, Static.Instance );
		}
	}
}