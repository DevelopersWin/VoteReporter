namespace DragonSpark.Runtime.Assignments
{
	public class Assignment<T1, T2> : Disposable
	{
		readonly IAssign<T1, T2> assign;
		readonly Value<T1> first;
		readonly Value<T2> second;

		public Assignment( IAssign<T1, T2> assign, T1 first, T2 second ) : this( assign, Assignments.From( first ), new Value<T2>( second ) ) {}

		public Assignment( IAssign<T1, T2> assign, Value<T1> first, Value<T2> second )
		{
			this.assign = assign;
			this.first = first;
			this.second = second;

			assign.Assign( first.Start, second.Start );
		}

		protected override void OnDispose( bool disposing ) => assign.Assign( first.Finish, second.Finish );
	}
}