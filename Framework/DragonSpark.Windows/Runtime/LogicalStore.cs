using DragonSpark.Runtime.Values;
using System.Runtime.Remoting.Messaging;

namespace DragonSpark.Windows.Runtime
{
	public class LogicalStore<T> : WritableStore<T>
	{
		readonly string slot;

		public LogicalStore( string slot )
		{
			this.slot = slot;
		}

		public override void Assign( T item )
		{
			if ( item == null )
			{
				CallContext.FreeNamedDataSlot( slot );
			}
			else
			{
				CallContext.LogicalSetData( slot, item );
			}
		}

		protected override T Get() => (T)CallContext.LogicalGetData( slot );
	}
}