using DragonSpark.TypeSystem;
using System;
using System.Collections.Generic;
using Xunit;

namespace DragonSpark.Testing.TypeSystem
{
	public class TypeAdapterTests
	{
		[Fact]
		public void EnumerableType()
		{
			var item = new TypeAdapter( typeof(List<int>) ).GetEnumerableType();
			Assert.Equal( typeof(int), item );
		}

		[Fact]
		public void IsInstanceOfType()
		{
			var adapter = new TypeAdapter( typeof(Casted) );
			var instance = new Casted( 6776 );
			Assert.True( adapter.IsInstanceOfType( instance ) );
			Assert.Equal( 6776, instance.Item );
		}

		[Fact]
		public void Throws()
		{
			Assert.Throws<ArgumentNullException>( () => Create() );
		}

		TypeAdapter Create() => new TypeAdapter( null, null );

		class Casted
		{
			public Casted( int item )
			{
				Item = item;
			}

			public int Item { get; }

			/*public static implicit operator Casted( int item )
			{
				return new Casted( item );
			}*/
		}
	}
}