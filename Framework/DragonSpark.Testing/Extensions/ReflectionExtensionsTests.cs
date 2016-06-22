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
			typeof(Static).Adapt().GenericMethods.Invoke( nameof(Static.Assign), new [] { typeof(Class) }, new object[] { null } );
			
			Assert.Null( Static.Instance );
			
			typeof(Static).Adapt().GenericMethods.Invoke( nameof(Static.Assign), new [] { typeof(Class) }, @class );

			Assert.Equal( @class, Static.Instance );
		}
	}
}