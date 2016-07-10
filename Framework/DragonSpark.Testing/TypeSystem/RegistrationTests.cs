﻿using DragonSpark.Activation;
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

		class AssemblySource : AssemblyStoreBase
		{
			readonly internal static Assembly[] Result = new Assembly[0];

			public AssemblySource() : base( Result ) {}
		}

		[Fact]
		public void IsFactory()
		{
			var result = FactoryInterfaceLocator.Instance.Get( typeof(UnityContainerFactory) );
			Assert.Equal( typeof(IFactory<IUnityContainer>), result );

			var implemented = ImplementedInterfaceFromConventionLocator.Instance.Get( typeof(UnityContainerFactory) );
			Assert.Null( implemented );
		}
	}
}