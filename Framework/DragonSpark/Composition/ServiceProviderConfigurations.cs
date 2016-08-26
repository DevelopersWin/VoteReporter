using System;
using System.Collections.Generic;
using System.Windows.Input;
using DragonSpark.Sources;
using DragonSpark.Sources.Parameterized;

namespace DragonSpark.Composition
{
	public class ServiceProviderConfigurations : Application.Setup.ServiceProviderConfigurations
	{
		public static ServiceProviderConfigurations Default { get; } = new ServiceProviderConfigurations();
		ServiceProviderConfigurations() : this( ServiceProviderSource.Default.Get ) {}

		readonly Func<IServiceProvider> source;

		protected ServiceProviderConfigurations( Func<IServiceProvider> source ) : this( source, InitializeExportsCommand.Default.Execute ) {}

		ServiceProviderConfigurations( Func<IServiceProvider> source, Action<IServiceProvider> configure )
		{
			this.source = new ConfiguringFactory<IServiceProvider>( source, configure ).Get;
		}

		protected override IEnumerable<ICommand> Yield()
		{
			yield return Application.Setup.ServiceProviderFactory.Default.Seed.Configured( source );
			foreach ( var command in base.Yield() )
			{
				yield return command;
			}
		}
	}
}