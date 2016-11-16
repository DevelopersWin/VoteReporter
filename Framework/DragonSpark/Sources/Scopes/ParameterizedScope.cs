using DragonSpark.Sources.Parameterized;
using System;

namespace DragonSpark.Sources.Scopes
{
	public class ParameterizedScope<TParameter, TResult> : Scope<Func<TParameter, TResult>>, IParameterizedScope<TParameter, TResult>
	{
		public ParameterizedScope() : this( parameter => default(TResult) ) {}
		public ParameterizedScope( Func<TParameter, TResult> source ) : base( source.Self ) {}
		public ParameterizedScope( Func<object, Func<TParameter, TResult>> source ) : base( source ) {}

		public TResult Get( TParameter parameter ) => Get().Invoke( parameter );
	}
}