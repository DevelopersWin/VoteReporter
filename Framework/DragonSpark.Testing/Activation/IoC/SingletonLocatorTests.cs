using DragonSpark.Activation;
using DragonSpark.Activation.IoC;
using DragonSpark.Extensions;
using DragonSpark.Runtime;
using DragonSpark.Runtime.Stores;
using DragonSpark.Setup;
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
			Assert.Same( SingletonItem.Instance, sut.Get( typeof(SingletonItem) ) );
		}

		[Fact]
		public void SingletonFromMetadata()
		{
			var sut = SingletonLocator.Instance;
			Assert.Same( SingletonMetadata.AnotherNameFromDefault, sut.Get( typeof(SingletonMetadata) ) );
		}

		[Fact]
		public void SingletonFromDifferentType()
		{
			var sut = SingletonLocator.Instance;
			Assert.Null( sut.Get( typeof(SingletonDifferentType) ) );
		}

		[Fact]
		public void SingletonFromOther()
		{
			var sut = new SingletonLocator( new SingletonDelegates( new SingletonSpecification( nameof(SingletonOther.Other) ) ).Get );
			Assert.Same( SingletonOther.Other, sut.Get( typeof(SingletonOther) ) );
		}

		[Fact]
		public void SingletonFromConvention()
		{
			var nestedTypes = GetType().GetTypeInfo().DeclaredNestedTypes.AsTypes().ToArray();
			var sut = SingletonLocator.Instance;
			var type = new ConventionTypes( new TypeSource( nestedTypes ) ).Get( typeof(ISingleton) ) ?? typeof(ISingleton);
			Assert.Same( Singleton.Instance, sut.Get( type ) );
		}

		[Fact]
		public void SingletonFromSource()
		{
			var item = SingletonLocator.Instance.Get( typeof(SourceSingleton) );
			Assert.NotNull( item );
			Assert.Same( item, SourceSingleton.Instance.Get() );
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

		class SourceSingleton
		{
			public static ISource<SourceSingleton> Instance { get; } = new FixedStore<SourceSingleton>( new SourceSingleton() );
		}
	}
}