using System.Runtime.InteropServices;

namespace DragonSpark.Aspects
{
	public abstract class InvocationBase<TParameter, TResult> : IInvocation<TParameter, TResult>
	{
		public abstract TResult Invoke( TParameter parameter );

		object IInvocation.Invoke( [Optional] object parameter ) =>
			parameter is TParameter ? (object)Invoke( (TParameter)parameter ) ?? parameter : parameter;

	}

	/*public class DelegatedInvocation<TParameter, TResult> : InvocationBase<TParameter, TResult>, IInvocation
	{
		readonly IInvocation inner;
		public DelegatedInvocation( IInvocation inner )
		{
			this.inner = inner;
		}

		public object Invoke( object parameter ) => inner.Invoke( parameter );
	}*/
}