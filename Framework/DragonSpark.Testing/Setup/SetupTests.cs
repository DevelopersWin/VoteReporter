using DragonSpark.Activation;
using DragonSpark.Extensions;
using DragonSpark.Setup;
using Xunit;

namespace DragonSpark.Testing.Setup
{
	public class SetupTests
	{
		[Fact]
		public void ServiceProviderCaching()
		{
			var count = 0;
			Exports.Instance.Assign( () =>
									 {
										++count;
										return DefaultExportProvider.Instance;
									 }  );
			Assert.Equal( 0, count );

			var serviceProvider = DefaultServiceProvider.Instance.Cached();
			serviceProvider.Get<IExportProvider>();
			Assert.Equal( 1, count );
			serviceProvider.Get<IExportProvider>();
			Assert.Equal( 1, count );

			var provider = ServiceProviderFactory.Instance.Get();
			Assert.Equal( 2, count );

			provider.Get<IExportProvider>();
			Assert.Equal( 3, count );

			for ( var i = 0; i < 10; i++ )
			{
				provider.Get<IExportProvider>();
				Assert.Equal( 3, count );
			}
		}
	}
}