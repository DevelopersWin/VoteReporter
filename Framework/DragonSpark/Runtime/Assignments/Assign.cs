using DragonSpark.Sources;

namespace DragonSpark.Runtime.Assignments
{
	public class Assign<T> : IAssign<T>
	{
		readonly IAssignable<T> assignable;
		public Assign( IAssignable<T> assignable )
		{
			this.assignable = assignable;
		}

		void IAssign<T>.Assign( T first ) => assignable.Assign( first );
	}
}