using DragonSpark.Testing.Objects.IoC;
using Ploeh.AutoFixture.Xunit2;
using PostSharp.Patterns.Model;
using System;
using Xunit;
using ServiceLocator = DragonSpark.Activation.IoC.ServiceLocator;

namespace DragonSpark.Testing.Activation.IoC
{
	[UnityContainerFactory.Register]
	public class DefaultUnityInstancesTests
	{
		[Theory, Framework.Setup.AutoData]
		void Container( [Modest, Frozen] ServiceLocator sut )
		{
			sut.QueryInterface<IDisposable>().Dispose();

			Assert.Throws<ObjectDisposedException>( () => sut.Container );
		}
	}
}