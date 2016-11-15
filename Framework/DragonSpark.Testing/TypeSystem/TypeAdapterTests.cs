using DragonSpark.Extensions;
using DragonSpark.Specifications;
using DragonSpark.Testing.Objects;
using DragonSpark.TypeSystem;
using PostSharp.Aspects;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace DragonSpark.Testing.TypeSystem
{
	public class TypeAdapterTests
	{
		[Fact]
		public void EnumerableType()
		{
			var item = typeof(List<int>).GetEnumerableType();
			Assert.Equal( typeof(int), item );
		}

		[Fact]
		public void GetMappedMethods()
		{
			var mappings = typeof(List<int>).GetMappedMethods( typeof(ICollection<int>) );
			Assert.NotEmpty( mappings );
			Assert.Equal( typeof(ICollection<int>), mappings.First().InterfaceMethod.DeclaringType);
			Assert.Equal( typeof(List<int>), mappings.First().MappedMethod.DeclaringType);

			Assert.Empty( typeof(List<int>).GetMappedMethods( typeof(ISpecification<int>) ) );
		}

		[Fact]
		public void Coverage_GenericMethods()
		{
			GetType().GetFactory( nameof(Generic) ).Make( typeof(int) );
		}

		public static Type Generic<T>( int number ) where T : IAspect => typeof(T);

		[Fact]
		public void IsInstanceOfType()
		{
			var instance = new Casted( 6776 );
			Assert.True( typeof(Casted).IsInstanceOfType( instance ) );
			Assert.Equal( 6776, instance.Item );
		}

		class Casted
		{
			public Casted( int item )
			{
				Item = item;
			}

			public int Item { get; }
		}

		[Fact]
		public void GetHierarchy()
		{
			Assert.Equal( new[]{ typeof(Derived), typeof(Class) }, typeof(Derived).GetHierarchy().ToArray() );
		}

		[Fact]
		public void GetAllInterfaces()
		{
			var interfaces = typeof(Derived).GetAllInterfaces().OrderBy( x => x.Name ).ToArray();
			Assert.Equal( new[]{ typeof(IAnotherInterface), typeof(IInterface) }, interfaces );
		}

		[Fact]
		public void GetItemType()
		{
			Assert.Equal( typeof(Class), typeof(List<Class>).GetInnerType() );
			Assert.Equal( typeof(Class), typeof(Class[]).GetInnerType() );
			Assert.Null( typeof(Class).GetInnerType() );
		}
	}
}