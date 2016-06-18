using DragonSpark.Runtime.Properties;
using DragonSpark.Runtime.Stores;

namespace DragonSpark.Configuration
{
	class ConfigurationStore<T> : ExecutionCachedStoreBase<T> where T : class, new()
	{
		public ConfigurationStore() : this( PrototypeStore<T>.Instance ) {}

		ConfigurationStore( PrototypeStore<T> store ) : base( new Cache( store ).Create ) {} // TODO: Fixist.

		class Cache
		{
			readonly PrototypeStore<T> store;
			public Cache( PrototypeStore<T> store )
			{
				this.store = store;
			}

			public T Create() => store.Register( new FixedStore<T>() );
		}
	}
}