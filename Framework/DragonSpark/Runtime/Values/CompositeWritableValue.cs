using DragonSpark.Extensions;

namespace DragonSpark.Runtime.Values
{
	public class CompositeWritableValue<T> : FixedValue<T>
	{
		readonly IWritableValue<T>[] values;

		public CompositeWritableValue( params IWritableValue<T>[] values )
		{
			this.values = values;
		}

		protected override void OnAssign( T item )
		{
			values.Each( value => value.Assign( item ) );
			base.OnAssign( item );
		}
	}
}