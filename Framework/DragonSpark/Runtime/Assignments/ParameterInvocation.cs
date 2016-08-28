namespace DragonSpark.Runtime.Assignments
{
	public interface IAssign<in T>
	{
		void Assign( T first );
	}

	/*public class EnabledStateAssign : IAssign<object, bool>
	{
		readonly EnabledState value;

		public EnabledStateAssign( EnabledState value )
		{
			this.value = value;
		}

		public void Assign( object first, bool second ) => value.Enable( first, second );
	}*/

	public class Assignment<T> : DisposableBase
	{
		readonly IAssign<T> assign;
		readonly Value<T> first;
		
		public Assignment( IAssign<T> assign, Value<T> first )
		{
			this.assign = assign;
			this.first = first;
			
			assign.Assign( first.Start );
		}

		protected override void OnDispose( bool disposing ) => assign.Assign( first.Finish );
	}
}
