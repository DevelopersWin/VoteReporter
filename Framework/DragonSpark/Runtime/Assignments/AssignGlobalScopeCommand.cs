using DragonSpark.Sources;
using System;
using System.Collections.Generic;

namespace DragonSpark.Runtime.Assignments
{
	public class AssignGlobalItemScopeCommand<T> : AssignCommand<Func<object, IEnumerable<T>>>
	{
		public AssignGlobalItemScopeCommand( IAssignable<Func<object, IEnumerable<T>>> assignable ) : base( assignable ) {}
	}
	
	public class AssignGlobalScopeCommand<T> : AssignCommand<Func<object, T>>
	{
		public AssignGlobalScopeCommand( IAssignable<Func<object, T>> assignable ) : base( assignable ) {}
	}
}