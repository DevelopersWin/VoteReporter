using System.Runtime.InteropServices;
using DragonSpark.Commands;
using DragonSpark.Extensions;
using DragonSpark.Sources;

namespace DragonSpark.Runtime.Assignments
{
	public class AssignCommand<T> : DisposingCommand<T>
	{
		readonly IAssignable<T> assignable;
		readonly T current;

		public AssignCommand( IAssignableSource<T> store ) : this( store, store ) {}

		public AssignCommand( IAssignable<T> assignable, ISource<T> store ) : this( assignable, store.Get() ) {}

		public AssignCommand( IAssignable<T> assignable, [Optional]T current )
		{
			this.assignable = assignable;
			this.current = current;
		}

		public override void Execute( T parameter ) => assignable.Assign( parameter );

		protected override void OnDispose()
		{
			assignable.TryDispose();
			assignable.Assign( current );
			base.OnDispose();
		}
	}
}