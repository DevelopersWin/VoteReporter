namespace DragonSpark.Configuration
{
	/*class ConfigurationStore<T> : ExecutionCachedStoreBase<T> where T : class, new()
	{
		public ConfigurationStore() : this( PrototypeStore<T>.Default ) {}

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
	}*/
}