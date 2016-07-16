using DragonSpark.Activation;
using DragonSpark.Extensions;
using DragonSpark.Runtime.Properties;
using DragonSpark.Runtime.Stores;
using DragonSpark.Testing.Objects;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace DragonSpark.Testing.Runtime.Values
{
	public class AmbientStackPropertyTests
	{
		readonly AmbientStackCache<Class> cache = new AmbientStackCache<Class>();

		/*IStack<Class> GetStack() => property.Get( Execution.Current );

		IEnumerable<Class> Items() => GetStack().All().ToArray();

		Class GetItem() => GetStack().Peek();*/

		[Fact]
		public void ContextAsExpected()
		{
			var stack = new AmbientStack<Class>( this.Self, cache );

			var expected = stack.Value;
			Assert.Same( expected, stack.Value );

			Assert.Null( stack.GetCurrentItem() );

			var first = new Class();
			using ( new AmbientStackCommand<Class>( stack ).Run( first ) )
			{
				Assert.Same( first, stack.GetCurrentItem() );

				var second = new Class();
				using ( new AmbientStackCommand<Class>( stack ).Run( second ) )
				{
					Assert.Same( second, stack.GetCurrentItem() );

					var third = new Class();
					using ( new AmbientStackCommand<Class>( stack ).Run( third ) )
					{
						Assert.Same( third, stack.GetCurrentItem() );

						var chain = stack.Value.All();
						Assert.Equal( 3, chain.Length );
						Assert.Same( chain.First(), third );
						Assert.Same( chain.Last(), first );

						var inside = new Class();
						var appended = inside.Append( stack.Value.All().ToArray() ).ToArray();
						Assert.Equal( 4, appended.Length );
						Assert.Same( appended.First(), inside );
						Assert.Same( appended.Last(), first );

						Task.Run( () =>
						{
							var thread = stack.Value;
							Assert.Same( thread, stack.Value );
							Assert.NotSame( expected, thread );
							Assert.Empty( stack.Value.All().ToArray() );
							var other = new Class();
							using ( new AmbientStackCommand<Class>( stack ).Run( other ) )
							{
								Assert.Same( other, stack.GetCurrentItem() );
								Assert.Single( stack.Value.All().ToArray(), other );
							}
							Assert.Same( thread, stack.Value );
						} ).Wait();
					}

					Assert.Same( second, stack.GetCurrentItem() );
				}

				Assert.Single( stack.Value.All().ToArray(), first );
				Assert.Same( first, stack.GetCurrentItem() );
			}

			Assert.Null( stack.GetCurrentItem() );

			Assert.NotSame( expected, stack.Value );
		} 
	}
}