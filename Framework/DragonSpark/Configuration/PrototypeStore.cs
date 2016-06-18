using DragonSpark.Activation;
using DragonSpark.Extensions;
using DragonSpark.Runtime.Stores;
using System.Collections.Concurrent;

namespace DragonSpark.Configuration
{
	// [ReaderWriterSynchronized]
	class PrototypeStore<T> : FixedStore<T> where T : class, new()
	{
		public static PrototypeStore<T> Instance { get; } = new PrototypeStore<T>();

		// [Reference]
		readonly IActivator activator;

		PrototypeStore() : this( Activator.Instance ) {}

		public PrototypeStore( IActivator activator )
		{
			this.activator = activator;
		}

		// [Writer]
		protected override void OnAssign( T item )
		{
			base.OnAssign( item );
			Copies.Each( Assign );
		}

		void Assign( IWritableStore<T> store ) => store.Assign( Value );

		protected override T Get() => base.Get() ?? /*activator.Activate<T>()*/ new T().With( Assign );

		// [Writer]
		public T Register( IWritableStore<T> store )
		{
			Copies.Add( store.With( Assign ) );
			return store.Value;
		}

		// [Reference]
		ConcurrentBag<IWritableStore<T>> Copies { get; } = new ConcurrentBag<IWritableStore<T>>();

		// [Writer]
		protected override void OnDispose()
		{
			base.OnDispose();
			while ( !Copies.IsEmpty )
			{
				IWritableStore<T> item;
				Copies.TryTake( out item );
			}
			// Copies.Clear();
		}
	}
}