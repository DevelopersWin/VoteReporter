using DragonSpark.Activation;
using DragonSpark.Activation.FactoryModel;
using DragonSpark.Extensions;
using DragonSpark.Testing.Framework.Parameters;
using DragonSpark.Testing.Objects;
using Ploeh.AutoFixture.Xunit2;
using Xunit;

namespace DragonSpark.Testing.Activation
{
	[Freeze( typeof(IActivator), typeof(SystemActivator) )]
	public class FactoryTests
	{
		[Theory, Framework.Setup.AutoData]
		void CreateActivation( [Modest]ActivateFactory<Class> sut )
		{
			var creation = sut.To<IFactoryWithParameter>().Create( typeof(Class) );
			Assert.NotNull( creation );
			Assert.IsType<Class>( creation );
		}

		[Theory, Framework.Setup.AutoData]
		void Create( [Modest]ConstructFactory<IInterface> sut )
		{
			var factory = sut.To<IFactoryWithParameter>();
			var result = factory.Create( typeof(Class) );
			Assert.IsType<Class>( result );

			/*var @class = factory.Create( null );
			Assert.NotNull( @class );*/
		}
	}																	        
}