using DragonSpark.Setup;

namespace DragonSpark.Windows.Setup
{
	public class ConsoleApplication : Application<string[]>
	{
		// public ConsoleApplication( ConfigureLocationCommand inner ) : base( inner ) {}
	}

	/*public class ConfigurationFactory : FactoryBase<Func<string, object>>
	{
		public static ConfigurationFactory Instance { get; } = new ConfigurationFactory();

		protected override Func<string, object> CreateItem() => ;
	}*/
}