using DragonSpark.Activation;
using DragonSpark.Extensions;
using DragonSpark.Runtime.Values;
using PostSharp.Patterns.Model;
using PostSharp.Patterns.Threading;
using System.Collections.Generic;

namespace DragonSpark.Configuration
{
	[Synchronized]
	class PrototypeStore<T> : FixedStore<T> where T : class, new()
	{
		public static PrototypeStore<T> Instance { get; } = new PrototypeStore<T>();

		[Reference]
		readonly IActivator activator;

		PrototypeStore() : this( Activator.Instance ) {}

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

		[Reference]
		ICollection<IWritableStore<T>> Copies { get; } = new List<IWritableStore<T>>();

		protected override void OnDispose()
		{
			base.OnDispose();
			Copies.Clear();
		}
	}
}