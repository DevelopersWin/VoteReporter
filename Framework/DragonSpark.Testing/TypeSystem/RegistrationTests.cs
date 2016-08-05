using DragonSpark.Activation;
using DragonSpark.Activation.IoC;
using DragonSpark.Testing.Framework;
using DragonSpark.Testing.Framework.Setup;
using DragonSpark.TypeSystem;
using Microsoft.Practices.Unity;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Reflection;
using Xunit;
using UnityContainerFactory = DragonSpark.Activation.IoC.UnityContainerFactory;

namespace DragonSpark.Testing.TypeSystem
{
	[ContainingTypeAndNested]
	public class RegistrationTests
	{
		// [RegisterFactory( typeof(AssemblySource) )]
		[Theory, Framework.Setup.AutoData]
		[Trait( Traits.Category, Traits.Categories.ServiceLocation )]
		public void Testing( ImmutableArray<Assembly> sut )
		{
			Assert.Equal( AssemblySource.Result, sut );
		}

		[Export]
		class AssemblySource : AssemblyBasedTypeSource
		{
			readonly internal static IEnumerable<Assembly> Result = EnumerableEx.Return( typeof(AssemblySource).Assembly );

			public AssemblySource() : base( Result ) {}
		}

		[Fact]
		public void IsFactory()
		{
			var result = SourceInterfaces.Instance.Get( typeof(UnityContainerFactory) );
			Assert.Equal( typeof(IFactory<IUnityContainer>), result );

			var implemented = ConventionImplementedInterfaces.Instance.Get( typeof(UnityContainerFactory) );
			Assert.Null( implemented );
		}
	}
}