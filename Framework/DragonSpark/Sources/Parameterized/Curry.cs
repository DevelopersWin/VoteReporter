using System;

namespace DragonSpark.Sources.Parameterized
{
	public class Curry<T1, T2, TResult> : DelegatedParameterizedSource<T1, Func<T2, TResult>>, ICurry<T1, T2, TResult>
	{
		public Curry( Func<T1, Func<T2, TResult>> factory ) : base( factory ) {}
	}
}