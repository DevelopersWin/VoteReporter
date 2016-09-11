namespace DragonSpark.Aspects.Extensibility
{
	class WrappedInvocation<TParameter, TResult> : IInvocation<TParameter, TResult>
	{
		readonly IInvocation invocation;

		public WrappedInvocation( IInvocation invocation )
		{
			this.invocation = invocation;
		}

		TResult IInvocation<TParameter, TResult>.Invoke( TParameter parameter ) => (TResult)Invoke( parameter );

		public object Invoke( object parameter ) => invocation.Invoke( parameter );
	}
}