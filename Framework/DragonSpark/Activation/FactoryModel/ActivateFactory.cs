namespace DragonSpark.Activation.FactoryModel
{
	public class ActivateFactory<TResult> : ActivationFactory<ActivateParameter, TResult> where TResult : class
	{
		public ActivateFactory( IActivator activator ) : this( activator, ActivateFactoryParameterCoercer<TResult>.Instance ) {}

		ActivateFactory( IActivator activator, IFactoryParameterCoercer<ActivateParameter> coercer ) : base( activator, coercer ) {}

		protected override TResult Activate( ActivateParameter parameter ) => Activator.Activate<TResult>( parameter.Type, parameter.Name );
	}
}