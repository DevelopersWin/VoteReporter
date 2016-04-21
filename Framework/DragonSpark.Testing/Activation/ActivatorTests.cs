using DragonSpark.Activation;
using DragonSpark.Testing.Framework;
using DragonSpark.Testing.Objects;
using Xunit;
using Xunit.Abstractions;
using Constructor = DragonSpark.Activation.Constructor;

namespace DragonSpark.Testing.Activation
{
	public class ActivatorTests : TestCollectionBase
	{
		public ActivatorTests( ITestOutputHelper output ) : base( output ) {}

		[Fact]
		public void Default()
		{
			var activator = Services.Get<IActivator>();
			Assert.Same( Activator.Instance, activator );
			var instance = activator.Activate<IInterface>( typeof(Class) );
			Assert.IsType<Class>( instance );
		}

		[Theory, Ploeh.AutoFixture.Xunit2.AutoData]
		public void DefaultCreate( string parameter )
		{
			var activator = Services.Get<IActivator>();
			Assert.Same( Activator.Instance, activator );
			
			var instance = activator.Construct<ClassWithParameter>( parameter );
			Assert.NotNull( instance );
			Assert.Equal( parameter, instance.Parameter );
		}
	}
}