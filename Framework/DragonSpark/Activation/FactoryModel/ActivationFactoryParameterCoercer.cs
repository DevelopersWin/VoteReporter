using System;

namespace DragonSpark.Activation.FactoryModel
{
	public abstract class ActivationFactoryParameterCoercer<TParameter, TResult> : FactoryParameterCoercer<TParameter>
	{
		protected override TParameter PerformCoercion( object context ) => Create( context as Type ?? typeof(TResult), context is Type ? null : context );

		protected abstract TParameter Create( Type type, object parameter );
	}
}