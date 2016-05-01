using System.Collections.Generic;
using DragonSpark.Activation;
using DragonSpark.Extensions;
using DragonSpark.Runtime.Values;

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
}