using DragonSpark.Extensions;
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
	public class InitializeConfigurationCommand : ServiceAssignedCommand<ConfigureCommand, IList<IWritableStore>>
	{
		public InitializeConfigurationCommand() : base( new OnlyOnceSpecification() ) {}

		public Collection<IWritableStore> Configurations { get; } = new Collection<IWritableStore>();

		public override object GetParameter() => Configurations.ToArray();
	}

	public class ConfigureCommand : CommandBase<IEnumerable<IWritableStore>>
	{
		public static ConfigureCommand Instance { get; } = new ConfigureCommand();

		ConfigureCommand() {}

		protected override void OnExecute( IEnumerable<IWritableStore> parameter ) => parameter.Each( store =>
		{
			GetType().InvokeGenericAction( nameof(Add), store.GetType().ToItem(), store );
		} );

		static void Add<T>( T store ) where T : class, IWritableStore, new() => PrototypeStore<T>.Instance.Assign( store );
	}
}
