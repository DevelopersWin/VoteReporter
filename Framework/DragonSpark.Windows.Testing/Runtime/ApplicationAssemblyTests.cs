using DragonSpark.Application;
using DragonSpark.Application.Setup;
using DragonSpark.Extensions;
using System;
using System.Reflection;
using Xunit;
using ApplicationAssemblyLocator = DragonSpark.Windows.Runtime.ApplicationAssemblyLocator;

namespace DragonSpark.Windows.Testing.Runtime
{
	public class ApplicationAssemblyTests
	{
		[Theory, Ploeh.AutoFixture.Xunit2.AutoData]
		public void Create()
		{
			new AssignSystemPartsCommand( GetType() ).Run();

			Assert.Equal( GetType().Assembly, ApplicationAssembly.Default.Get() );
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