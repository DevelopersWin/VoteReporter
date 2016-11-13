namespace DragonSpark.Runtime.Assignments
{
	public class Assignment<T> : DisposableBase
	{
		readonly IAssign<T> assign;
		readonly T finish;

		public Assignment( IAssign<T> assign, T finish )
		{
			this.assign = assign;
			this.finish = finish;
		}

		protected override void OnDispose( bool disposing ) => assign.Assign( finish );
	}
}