using DragonSpark.Runtime;
using DragonSpark.Runtime.Specifications;
using DragonSpark.Runtime.Values;
using DragonSpark.Setup.Commands;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Markup;

namespace DragonSpark.Configuration
{
	public class EnableMethodCaching : ConfigurationBase<bool>
	{
		public EnableMethodCaching() : base( true ) {}
	}

	public static class Configure
	{
		public static T Load<T>() where T : class, IWritableStore, new() => new ConfigurationStore<T>().Value;

		public static TValue Get<TConfiguration, TValue>() where TConfiguration : ConfigurationBase<TValue>, new() => Load<TConfiguration>().Value;
	}

	public abstract class ConfigurationBase<T> : PropertyStore<T>
	{
		protected ConfigurationBase( T value )
		{
			Value = value;
		}
	}

	[ContentProperty( nameof(Configurations) )]
	public class InitializeConfigurationCommand : ServicedCommand<ConfigureCommand, IList<IWritableStore>>
	{
		public InitializeConfigurationCommand() : base( new OnlyOnceSpecification().Box<object>() ) {}

		public Collection<IWritableStore> Configurations { get; } = new Collection<IWritableStore>();

		public override object GetParameter() => Configurations.ToArray();
	}
}
