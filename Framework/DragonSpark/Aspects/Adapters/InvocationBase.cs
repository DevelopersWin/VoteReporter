using System.Runtime.InteropServices;

namespace DragonSpark.Aspects.Adapters
{
	public abstract class InvocationBase<TParameter, TResult> : IInvocation
	{
		public abstract TResult Invoke( TParameter parameter );

		public object Get( [Optional] object parameter ) =>
			parameter is TParameter ? (object)Invoke( (TParameter)parameter ) ?? parameter : parameter;
	}
}