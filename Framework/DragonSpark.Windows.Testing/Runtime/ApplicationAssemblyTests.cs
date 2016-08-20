using DragonSpark.Extensions;
using DragonSpark.Setup;
using DragonSpark.TypeSystem;
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