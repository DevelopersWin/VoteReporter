using DragonSpark.Runtime.Values;
using System;
using System.Threading;

namespace DragonSpark.Windows.Runtime
{
	public class ThreadDataStore<T> : WritableStore<T>
	{
		readonly LocalDataStoreSlot slot;

		public ThreadDataStore( string key ) : this( Thread.GetNamedDataSlot( key ) ) {}

		public ThreadDataStore( LocalDataStoreSlot slot )
		{
			this.slot = slot;
		}

		public override void Assign( T item ) => Thread.SetData( slot, item );

		protected override T Get() => (T)Thread.GetData( slot );
	}
}