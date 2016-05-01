using DragonSpark.Runtime.Values;

namespace DragonSpark.Configuration
{
	class ConfigurationStore<T> : ExecutionContextStore<T> where T : class, new()
	{
		public ConfigurationStore() : this( PrototypeStore<T>.Instance ) {}

		public ConfigurationStore( PrototypeStore<T> store ) : base( () => store.Register( new FixedStore<T>() ) ) {}
	}

	/*[ReaderWriterSynchronized]
	class Store<T> : RepositoryBase<T> where T : class, IWritableStore
	{
		public static Store<T> Instance { get; } = new Store<T>();

		[Reference]
		readonly IActivator activator;

		public Store() : this( Constructor.Instance ) {}

		public Store( IActivator activator )
		{
			this.activator = activator;
		}

		[Writer]
		public T Create()
		{
			// var prototype = Store.FirstOrDefaultOfType<T>() ?? ;
			var result = activator.Activate<T>();
			// result.Assign( prototype.Value );
			return result;
		}

		[Writer]
		protected override void OnAdd( T entry )
		{
			var type = entry.GetType();
			Store.Where( type.Adapt().IsInstanceOfType ).ToArray().Each( Store.Remove );

			base.OnAdd( entry );
		}
	}*/
}