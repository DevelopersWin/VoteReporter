using DragonSpark.Runtime.Stores;
using DragonSpark.Windows.Runtime;
using System;
using System.Reflection;
using Xunit;

namespace DragonSpark.Windows.Testing.Runtime
{
	public class ApplicationAssemblyLocatorTests
	{
		[Theory, Ploeh.AutoFixture.Xunit2.AutoData]
		public void Create( ApplicationAssemblyLocator sut )
		{
			var assembly = sut.Get();
			Assert.Equal( GetType().Assembly, assembly );
		}

		[Theory, Ploeh.AutoFixture.Xunit2.AutoData]
		public void Other( Assembly[] assemblies )
		{
			var sut = new ApplicationAssemblyLocator( assemblies, AppDomain.CreateDomain( "NotAnAssembly" ) );
			var assembly = sut.Get();
			Assert.Null( assembly );
		}
	}
}