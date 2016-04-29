using DragonSpark.Activation;
using DragonSpark.Extensions;
using DragonSpark.Runtime.Values;
using System.Collections.Generic;
using Activator = DragonSpark.Activation.Activator;

namespace DragonSpark.Configuration
{
	class PrototypeStore<T> : FixedStore<T> where T : class, new()
	{
		public static PrototypeStore<T> Instance { get; } = new PrototypeStore<T>();

		readonly IActivator activator;

		public PrototypeStore() : this( Activator.Instance ) {}

		public PrototypeStore( IActivator activator )
		{
			this.activator = activator;
		}

		protected override void OnAssign( T item )
		{
			base.OnAssign( item );
			Copies.Each( Assign );
		}

		void Assign( IWritableStore<T> store ) => store.Assign( Value );

		protected override T Get() => base.Get() ?? /*activator.Activate<T>()*/ new T().With( Assign );

		public T Register( IWritableStore<T> store )
		{
			Copies.Add( store.With( Assign ) );
			return store.Value;
		}

		ICollection<IWritableStore<T>> Copies { get; } = new List<IWritableStore<T>>();

		protected override void OnDispose()
		{
			base.OnDispose();
			Copies.Clear();
		}
	}

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