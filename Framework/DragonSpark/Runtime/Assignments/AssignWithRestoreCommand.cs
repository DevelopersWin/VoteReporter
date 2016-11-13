using DragonSpark.Sources;

namespace DragonSpark.Runtime.Assignments
{
	public class AssignWithRestoreCommand<T> : AssignWithDisposeCommand<T>
	{
		public AssignWithRestoreCommand( IAssignable<T> assignable, T current ) : base( assignable, new Assignment<T>( new Assign<T>( assignable ), current ) ) {}
	}
}