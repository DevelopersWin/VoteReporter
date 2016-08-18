using DragonSpark.Extensions;
using DragonSpark.Testing.Framework;
using DragonSpark.Testing.Framework.Setup;
using DragonSpark.TypeSystem;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Reflection;
using Xunit;

namespace DragonSpark.Testing.TypeSystem
{
	[ContainingTypeAndNested]
	public class RegistrationTests
	{
		[Theory, AutoData]
		[Trait( Traits.Category, Traits.Categories.ServiceLocation )]
		public void Testing( ImmutableArray<Assembly> sut )
		{
			Assert.Equal( AssemblySource.Result, sut );
		}

		[Export]
		class AssemblySource : AssemblyBasedTypeSource
		{
			readonly internal static IEnumerable<Assembly> Result = typeof(AssemblySource).Assembly.Yield();

			public AssemblySource() : base( Result ) {}
		}

		/*[Fact]
		public void IsFactory()
		{
			var result = SourceInterfaces.Instance.Get( typeof(UnityContainerFactory) );
			Assert.Equal( typeof(ISource<IUnityContainer>), result );

			var implemented = ConventionImplementedInterfaces.Instance.Get( typeof(UnityContainerFactory) );
			Assert.Null( implemented );
		}*/
	}
}