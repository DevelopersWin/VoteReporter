using System;

namespace DragonSpark.Activation
{
	public abstract class TypeRequestCoercer<TParameter, TResult> : FactoryParameterCoercer<TParameter>
	{
		protected override TParameter PerformCoercion( object context ) => Create( context as Type ?? typeof(TResult), context is Type ? null : context );

		protected abstract TParameter Create( Type type, object parameter );
	}
}