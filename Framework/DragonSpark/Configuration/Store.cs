using DragonSpark.Runtime.Properties;
using DragonSpark.Runtime.Stores;

namespace DragonSpark.Configuration
{
	class ConfigurationStore<T> : ExecutionAttachedPropertyStoreBase<T> where T : class, new()
	{
		public ConfigurationStore() : this( PrototypeStore<T>.Instance ) {}

		public ConfigurationStore( PrototypeStore<T> store ) : base( () => store.Register( new FixedStore<T>() ) ) {}
	}
}