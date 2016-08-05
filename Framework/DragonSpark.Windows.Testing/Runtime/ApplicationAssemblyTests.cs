using DragonSpark.Windows.Runtime;
using System;
using System.Reflection;
using Xunit;

namespace DragonSpark.Windows.Testing.Runtime
{
	public class ApplicationAssemblyTests
	{
		[Theory, Ploeh.AutoFixture.Xunit2.AutoData]
		public void Create()
		{
			Assert.Equal( GetType().Assembly, ApplicationAssembly.Instance.Get() );
		}

		[Theory, Ploeh.AutoFixture.Xunit2.AutoData]
		public void Other( Assembly[] assemblies )
		{
			var sut = new ApplicationAssemblyLocator( AppDomain.CreateDomain( "NotAnAssembly" ) );
			var assembly = sut.Get( assemblies );
			Assert.Null( assembly );
		}
	}
}