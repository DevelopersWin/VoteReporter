using DragonSpark.Activation;
using DragonSpark.Extensions;
using DragonSpark.Runtime.Values;
using DragonSpark.Testing.Objects;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace DragonSpark.Testing.Runtime.Values
{
	public class AmbientStackPropertyTests
	{
		readonly AmbientStackProperty<Class> property = new AmbientStackProperty<Class>();

		IStack<Class> GetStack() => property.Get( Execution.Current );

		IEnumerable<Class> Items() => GetStack().All().ToArray();

		Class GetItem() => GetStack().Peek();

		[Fact]
		public void ContextAsExpected()
		{
			var stack = GetStack();
			Assert.Same( stack, GetStack() );

			Assert.Null( GetItem() );

			var first = new Class();
			using ( new AmbientStackCommand<Class>( property ).AsExecuted( first ) )
			{
				Assert.Same( first, GetItem() );

				var second = new Class();
				using ( new AmbientStackCommand<Class>( property ).AsExecuted( second ) )
				{
					Assert.Same( second, GetItem() );

					var third = new Class();
					using ( new AmbientStackCommand<Class>( property ).AsExecuted( third ) )
					{
						Assert.Same( third, GetItem() );

						var chain = GetStack().All();
						Assert.Equal( 3, chain.Length );
						Assert.Same( chain.First(), third );
						Assert.Same( chain.Last(), first );

						var inside = new Class();
						var appended = inside.Append( Items() ).ToArray();
						Assert.Equal( 4, appended.Length );
						Assert.Same( appended.First(), inside );
						Assert.Same( appended.Last(), first );

						Task.Run( () =>
						{
							var thread = GetStack();
							Assert.Same( thread, GetStack() );
							Assert.NotSame( stack, thread );
							Assert.Empty( Items() );
							var other = new Class();
							using ( new AmbientStackCommand<Class>( property ).AsExecuted( other ) )
							{
								Assert.Same( other, GetItem() );
								Assert.Single( Items(), other );
							}
							var one = GetStack();
							var two = GetStack();
							Assert.Same( thread, one );
						} ).Wait();
					}

					Assert.Same( second, GetItem() );
				}

				Assert.Single( GetStack().All().ToArray(), first );
				Assert.Same( first, GetItem() );
			}

			Assert.Null( GetItem() );

			Assert.NotSame( stack, GetStack() );
		} 
	}
}