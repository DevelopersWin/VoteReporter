using DragonSpark.Sources.Parameterized;
using System;

namespace DragonSpark.Sources.Scopes
{
	public class ParameterizedScope<TParameter, TResult> : Scope<Func<TParameter, TResult>>, IParameterizedScope<TParameter, TResult>
	{
		public ParameterizedScope() : this( parameter => default(TResult) ) {}
		public ParameterizedScope( Func<TParameter, TResult> source ) : base( source.Self ) {}
		public ParameterizedScope( Func<object, Func<TParameter, TResult>> source ) : base( source ) {}
		
		/*readonly IScope<Func<TParameter, TResult>> scope;

		
		public ParameterizedScope( Func<TParameter, TResult> source ) : this( new Scope<Func<TParameter, TResult>>( source.Self ) ) {}

		public ParameterizedScope( Func<object, Func<TParameter, TResult>> source ) : this( new Scope<Func<TParameter, TResult>>( source ) ) {}

		[UsedImplicitly]
		protected ParameterizedScope( IScope<Func<TParameter, TResult>> scope )
		{
			this.scope = scope;
		}*/

		// public override TResult Get( TParameter parameter ) => GetFactory().Invoke( parameter );

		/*public void Assign( ISourceAware item ) => scope.Assign( item );

		public virtual void Assign( Func<object, Func<TParameter, TResult>> item ) => scope.Assign( item );
		public virtual void Assign( Func<Func<TParameter, TResult>> item ) => scope.Assign( item );*/
		public TResult Get( TParameter parameter ) => Get().Invoke( parameter );
	}
}