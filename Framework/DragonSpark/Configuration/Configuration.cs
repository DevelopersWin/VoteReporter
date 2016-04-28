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
	class StoreStore<T, TValue> : ExecutionContextStore<T> where T : class, IStore<TValue>, IConfiguration
	{
		public StoreStore() : base( Store.Instance.Create<T> ) {}
	}

	public static class Load<T, TValue> where T : class, IWritableStore<TValue>, IConfiguration
	{
		public static TValue Get() => new StoreStore<T, TValue>().Value.Value;
	}

	public static class Assign<T, TValue> where T : class, IWritableStore<TValue>, IConfiguration
	{
		public static void With( TValue value ) => new StoreStore<T, TValue>().Value.Assign( value );
	}

	public interface IConfiguration : IStore
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
			Value = value;
		}

		public IConfiguration Clone() => (IConfiguration)MemberwiseClone();
	}

	[ContentProperty( nameof(Configurations) )]
	public class InitializeConfigurationCommand : ServiceAssignedCommand<ConfigureCommand, IList<IConfiguration>>
	{
		public InitializeConfigurationCommand() : base( new OnlyOnceSpecification() ) {}

		public Collection<IConfiguration> Configurations { get; } = new Collection<IConfiguration>();

		public override object GetParameter() => Configurations.ToArray();
	}

	public class ConfigureCommand : CommandBase<IEnumerable<IConfiguration>>
	{
		public static ConfigureCommand Instance { get; } = new ConfigureCommand();

		ConfigureCommand() {}

		protected override void OnExecute( IEnumerable<IConfiguration> parameter ) => parameter.Each( Store.Instance.Add );
	}
}
