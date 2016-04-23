using DragonSpark.ComponentModel;
using DragonSpark.Extensions;
using DragonSpark.Runtime;
using DragonSpark.Runtime.Specifications;
using DragonSpark.Runtime.Values;
using DragonSpark.Setup.Commands;
using System;
using System.Collections.Generic;
using System.Windows.Markup;

namespace DragonSpark.Configuration
{
	class ConfigurationValue<T> : ExecutionContextValue<T> where T : class, IConfiguration, new()
	{
		public ConfigurationValue() : base( Store.Instance.Create<T> ) {}
	}

	public static class Configure
	{
		public static void Initialize( IConfiguration configuration ) => Store.Instance.Add( configuration );

		public static T Get<T>() where T : class, IConfiguration, new() => new ConfigurationValue<T>().Item;

		public static Func<U> Get<T, U>( Func<T, U> get ) where T : class, IConfiguration, new() => () => get( Get<T>() );
	}

	public interface IConfiguration
	{
		IConfiguration Clone();
	}

	public abstract class ConfigurationBase : IConfiguration
	{
		public IConfiguration Clone() => (IConfiguration)MemberwiseClone();
	}

	public class Configuration : ConfigurationBase
	{
		[Default( true )]
		public bool EnableMethodCaching { get; set; }
	}

	[ContentProperty( nameof( Parameter ) )]
	public class InitializeConfigurationCommand : ServicedCommand<ConfigureCommand, IList<IConfiguration>>
	{
		public InitializeConfigurationCommand() : base( new OnlyOnceSpecification() ) {}
	}

	public class ConfigureCommand : Command<IEnumerable<IConfiguration>>
	{
		public static ConfigureCommand Instance { get; } = new ConfigureCommand();

		ConfigureCommand() {}

		protected override void OnExecute( IEnumerable<IConfiguration> parameter ) => parameter.Each( Configure.Initialize );
	}
}
