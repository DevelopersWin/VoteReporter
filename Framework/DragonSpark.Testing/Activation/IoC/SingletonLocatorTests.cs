using System.Linq;
using System.Reflection;
using DragonSpark.Activation.IoC;
using DragonSpark.Extensions;
using Xunit;

namespace DragonSpark.Testing.Activation.IoC
{
	public class SingletonLocatorTests
	{
		[Fact]
		public void SingletonFromItem()
		{
			var sut = new SingletonLocator();
			Assert.Same( SingletonItem.Instance, sut.Locate( typeof(SingletonItem) ) );
		}

		[Fact]
		public void SingletonFromMetadata()
		{
			var sut = new SingletonLocator();
			Assert.Same( SingletonMetadata.Temp, sut.Locate( typeof(SingletonMetadata) ) );
		}

		[Fact]
		public void SingletonFromOther()
		{
			var sut = new SingletonLocator( nameof(SingletonOther.Other) );
			Assert.Same( SingletonOther.Other, sut.Locate( typeof(SingletonOther) ) );
		}

		[Fact]
		public void SingletonFromConvention()
		{
			var nestedTypes = GetType().GetTypeInfo().DeclaredNestedTypes.AsTypes().ToArray();
			var conventionLocator = new BuildableTypeFromConventionLocator( nestedTypes );
			var sut = new SingletonLocator();
			var type = conventionLocator.Create( typeof(ISingleton) ) ?? typeof(ISingleton);
			Assert.Same( Singleton.Instance, sut.Locate( type ) );
		}

		class SingletonItem
		{
			public static SingletonItem Instance { get; } = new SingletonItem();
		}

		class SingletonMetadata
		{
			[Singleton]
			public static SingletonItem Temp { get; } = new SingletonItem();
		}

		class SingletonOther
		{
			public static SingletonOther Other { get; } = new SingletonOther();
		}

		class Singleton : ISingleton
		{
			public static Singleton Instance { get; } = new Singleton();
		}

		interface ISingleton
		{}
	}
}