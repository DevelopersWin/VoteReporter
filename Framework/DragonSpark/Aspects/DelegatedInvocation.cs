using DragonSpark.Sources.Parameterized;
using System;

namespace DragonSpark.Aspects
{
	public class DelegatedInvocation<TParameter, TResult> : InvocationBase<TParameter, TResult>
	{
		readonly Func<TParameter, TResult> source;

		public DelegatedInvocation( IParameterizedSource<TParameter, TResult> source ) : this( source.ToDelegate() ) {}

		public DelegatedInvocation( Func<TParameter, TResult> source )
		{
			this.source = source;
		}

		public override TResult Invoke( TParameter parameter ) => source( parameter );
	}
}