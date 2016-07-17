using DragonSpark.Activation;
using DragonSpark.Activation.IoC;
using DragonSpark.Extensions;
using System.Linq;
using System.Reflection;
using Xunit;

namespace DragonSpark.Testing.Activation.IoC
{
	public class SingletonLocatorTests
	{
		[Fact]
		public void SingletonFromItem()
		{
			var sut = SingletonLocator.Instance;
			Assert.Same( SingletonItem.Instance, sut.Locate( typeof(SingletonItem) ) );
		}

		[Fact]
		public void SingletonFromMetadata()
		{
			var sut = SingletonLocator.Instance;
			Assert.Same( SingletonMetadata.AnotherNameFromDefault, sut.Locate( typeof(SingletonMetadata) ) );
		}

		[Fact]
		public void SingletonFromDifferentType()
		{
			var sut = SingletonLocator.Instance;
			Assert.Null( sut.Locate( typeof(SingletonDifferentType) ) );
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
			var sut = SingletonLocator.Instance;
			var type = conventionLocator.Get( typeof(ISingleton) ) ?? typeof(ISingleton);
			Assert.Same( Singleton.Instance, sut.Locate( type ) );
		}

		class SingletonItem
		{
			public static SingletonItem Instance { get; } = new SingletonItem();
		}

		class SingletonMetadata
		{
			[Singleton]
			public static SingletonMetadata AnotherNameFromDefault { get; } = new SingletonMetadata();
		}

		class SingletonDifferentType
		{
			public static SingletonMetadata Instance { get; } = new SingletonMetadata();
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