using DragonSpark.Extensions;
using DragonSpark.Setup;
using DragonSpark.Testing.Framework;
using DragonSpark.TypeSystem;
using Moq;
using System.Collections.Immutable;
using Xunit;
using AutoDataAttribute = Ploeh.AutoFixture.Xunit2.AutoDataAttribute;

namespace DragonSpark.Testing.TypeSystem
{
	[Trait( Traits.Category, Traits.Categories.ServiceLocation )]
	public class ApplicationAssemblyFilterTests
	{
		[Theory, Framework.Setup.AutoData]
		public void Basic( Mock<ITypeSource> provider, ApplicationAssemblyFilter sut )
		{
			provider.Setup( p => p.Get() ).Returns( () => new[] { typeof(AutoDataAttribute), typeof(Framework.Setup.AutoDataAttribute) }.ToImmutableArray() );

			var enumerable = provider.Object.Get().Assemblies();
			var assemblies = sut.Get( enumerable );
			
			provider.Verify( assemblyProvider => assemblyProvider.Get() );
			Assert.NotEqual( assemblies, enumerable );
		}

		/*[AssemblyProvider.Register]
		[Theory, Framework.Setup.AutoData]
		public void DefaultProvider( ITypeSource sut )
		{
			Assert.NotNull( sut );
			Assert.Same( AssemblyProvider.Instance, sut );
		}*/

		[Theory, Framework.Setup.AutoData, DeclaredAssemblyProvider.Register]
		public void DeclaredProvider( ITypeSource sut )
		{
			Assert.NotNull( sut );
			Assert.Same( DeclaredAssemblyProvider.Instance, sut );
		}

		class DeclaredAssemblyProvider : AssemblySourceBase
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