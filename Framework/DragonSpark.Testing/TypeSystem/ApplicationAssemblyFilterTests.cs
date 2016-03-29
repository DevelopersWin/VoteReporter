using DragonSpark.Extensions;
using DragonSpark.Testing.Framework;
using DragonSpark.Testing.Objects;
using DragonSpark.TypeSystem;
using Moq;
using Xunit;
using AutoDataAttribute = Ploeh.AutoFixture.Xunit2.AutoDataAttribute;

namespace DragonSpark.Testing.TypeSystem
{
	[AssemblyProvider.Register]
	[AssemblyProvider.Types]
	public class ApplicationAssemblyFilterTests
	{
		[Theory, Framework.Setup.AutoData()]
		public void Basic( Mock<IAssemblyProvider> provider, ApplicationAssemblyFilter sut )
		{
			provider.Setup( p => p.Create() ).Returns( () => new[] { typeof(AutoDataAttribute), typeof(Framework.Setup.AutoDataAttribute) }.Assemblies() );

			var assemblies = sut.Create( provider.Object.Create() );
			
			provider.Verify( assemblyProvider => assemblyProvider.Create() );
			Assert.NotEqual( assemblies, provider.Object.Create() );
		}

		[Theory, Framework.Setup.AutoData]
		public void DefaultProvider( IAssemblyProvider sut )
		{
			Assert.NotNull( sut );
			Assert.Same( AssemblyProvider.Instance, sut );
		}

		[Theory, Framework.Setup.AutoData, DeclaredAssemblyProvider.Register]
		public void DeclaredProvider( IAssemblyProvider sut )
		{
			Assert.NotNull( sut );
			Assert.Same( DeclaredAssemblyProvider.Instance, sut );
		}

		class DeclaredAssemblyProvider : AssemblyProviderBase
		{
			public static DeclaredAssemblyProvider Instance { get; } = new DeclaredAssemblyProvider();

			public class Register : RegisterFactoryAttribute
			{
				public Register() : base( typeof(DeclaredAssemblyProvider) ) {}
			}

			DeclaredAssemblyProvider() : base( typeof(DeclaredAssemblyProvider) ) {}
		}
	}
}