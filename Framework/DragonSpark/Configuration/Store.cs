using DragonSpark.Runtime.Values;

namespace DragonSpark.Configuration
{
	class ConfigurationStore<T> : ExecutionContextStore<T> where T : class, new()
	{
		public ConfigurationStore() : this( PrototypeStore<T>.Instance ) {}

		public ConfigurationStore( PrototypeStore<T> store ) : base( () => store.Register( new FixedStore<T>() ) ) {}
	}
}