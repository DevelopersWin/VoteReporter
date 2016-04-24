using DragonSpark.Runtime.Values;
using System.Threading;

namespace DragonSpark.Windows.Runtime
{
	public class TaskLocalValue<T> : WritableValue<T>
	{
		readonly AsyncLocal<T> local;

		public TaskLocalValue() : this( new AsyncLocal<T>() )
		{}

		public TaskLocalValue( AsyncLocal<T> local )
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