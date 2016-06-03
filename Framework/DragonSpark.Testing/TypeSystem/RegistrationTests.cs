using DragonSpark.Activation;
using DragonSpark.Activation.IoC;
using DragonSpark.Testing.Framework;
using DragonSpark.TypeSystem;
using Microsoft.Practices.Unity;
using System.Reflection;
using Xunit;
using UnityContainerFactory = DragonSpark.Activation.IoC.UnityContainerFactory;

namespace DragonSpark.Testing.TypeSystem
{
	public class RegistrationTests
	{
		[RegisterFactory( typeof(AssemblySource) )]
		[Theory, Framework.Setup.AutoData]
		[Trait( Traits.Category, Traits.Categories.ServiceLocation )]
		public void Testing( Assembly[] sut )
		{
			Assert.Same( AssemblySource.Result, sut );
		}

		// [Export]
		class AssemblySource : AssemblySourceBase
		{
			readonly internal static Assembly[] Result = new Assembly[0];

			public override Assembly[] Create() => Result;
		}

		[Fact]
		public void IsFactory()
		{
			var result = FactoryInterfaceLocator.Instance.Create( typeof(UnityContainerFactory) );
			Assert.Equal( typeof(IFactory<IUnityContainer>), result );

			var implemented = ImplementedInterfaceFromConventionLocator.Instance.Create( typeof(UnityContainerFactory) );
			Assert.Null( implemented );
		}
	}
}