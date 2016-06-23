namespace DragonSpark.Runtime
{
	/*public class EnabledState
	{
		readonly static object Null = new object();

		readonly ISet<object> objects = new HashSet<object>();

		public bool IsEnabled( object item ) => objects.Contains( item ?? Null );

		public void Enable( object item, bool on )
		{
			var check = item ?? Null;

			if ( on )
			{
				objects.Add( check );
			}
			else
			{
				objects.Remove( check );
			}
		}
	}

	public static class EnabledStateExtensions
	{
		public static Assignment<object, bool> Assignment( this EnabledState @this, object first, bool second = true ) =>
			new Assignment<object, bool>( new EnabledStateAssign( @this ), Assignments.From( first ), new Value<bool>( second ) );
	}*/
}