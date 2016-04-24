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
	class StoreValue<T, TValue> : ExecutionContextValue<T> where T : class, IValue<TValue>, IConfiguration
	{
		public StoreValue() : base( Store.Instance.Create<T> ) {}
	}

	public static class Load<T, TValue> where T : class, IWritableValue<TValue>, IConfiguration
	{
		public static TValue Get() => new StoreValue<T, TValue>().Item.Item;
	}

	public static class Assign<T, TValue> where T : class, IWritableValue<TValue>, IConfiguration
	{
		public static void With( TValue value ) => new StoreValue<T, TValue>().Item.Assign( value );
	}

	public interface IConfiguration : IValue
	{
		IConfiguration Clone();
	}

	public class EnableMethodCaching : ConfigurationBase<bool>
	{
		public EnableMethodCaching() : base( true ) {}
	}

	public abstract class ConfigurationBase<T> : PropertyStore<T>, IConfiguration
	{
		protected ConfigurationBase( T value )
		{
			Item = value;
		}

		public IConfiguration Clone() => (IConfiguration)MemberwiseClone();
	}

	[ContentProperty( nameof(Configurations) )]
	public class InitializeConfigurationCommand : ServicedCommand<ConfigureCommand, IList<IConfiguration>>
	{
		public InitializeConfigurationCommand() : base( new OnlyOnceSpecification() ) {}

		public Collection<IConfiguration> Configurations { get; } = new Collection<IConfiguration>();

		public override IList<IConfiguration> Parameter => Configurations.ToArray();
	}

	public class ConfigureCommand : Command<IEnumerable<IConfiguration>>
	{
		public static ConfigureCommand Instance { get; } = new ConfigureCommand();

		ConfigureCommand() {}

		protected override void OnExecute( IEnumerable<IConfiguration> parameter ) => parameter.Each( Store.Instance.Add );
	}
}
