using DragonSpark.Runtime.Values;
using System.Threading;

namespace DragonSpark.Windows.Runtime
{
	public class TaskLocalStore<T> : WritableStore<T>
	{
		readonly AsyncLocal<T> local;

		public TaskLocalStore() : this( new AsyncLocal<T>() )
		{}

		public TaskLocalStore( AsyncLocal<T> local )
		{
			this.local = local;
		}

		public override void Assign( T item )
		{
			local.Value = item;
		}

		protected override T Get() => local.Value;
	}
}