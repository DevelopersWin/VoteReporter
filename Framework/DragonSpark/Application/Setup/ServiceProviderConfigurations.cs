using System.Collections.Generic;
using System.Windows.Input;
using DragonSpark.Activation.Location;
using DragonSpark.Commands;
using DragonSpark.Sources;

namespace DragonSpark.Application.Setup
{
	public class ServiceProviderConfigurations : CommandSource
	{
		// public static ServiceProviderConfigurations Default { get; } = new ServiceProviderConfigurations();
		protected ServiceProviderConfigurations() {}

		protected override IEnumerable<ICommand> Yield()
		{
			yield return GlobalServiceProvider.Default.Configured( ServiceProviderFactory.Default.ToFixedDelegate() );
		}
	}
}