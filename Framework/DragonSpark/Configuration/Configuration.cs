using DragonSpark.Aspects.Validation;
using DragonSpark.Runtime;
using DragonSpark.Runtime.Specifications;
using DragonSpark.Runtime.Stores;
using DragonSpark.Setup.Commands;
using PostSharp.Extensibility;
using System.Collections.Immutable;
using System.Windows.Markup;

namespace DragonSpark.Configuration
{
	public class EnableMethodCaching : ConfigurationBase<bool>
	{
		public EnableMethodCaching() : base( !PostSharpEnvironment.IsPostSharpRunning ) {}
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
	[ApplyAutoValidation]
	public class InitializeConfigurationCommand : ServicedCommand<ConfigureCommand, ImmutableArray<IWritableStore>>
	{
		public InitializeConfigurationCommand() : base( new OnlyOnceSpecification() ) {}

		public Collection<IWritableStore> Configurations { get; } = new Collection<IWritableStore>();

		public override ImmutableArray<IWritableStore> GetParameter() => Configurations.ToImmutableArray();
	}
}
