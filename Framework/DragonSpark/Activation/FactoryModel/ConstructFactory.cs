namespace DragonSpark.Activation.FactoryModel
{
	public class ConstructFactory<TResult> : ActivationFactory<ConstructParameter, TResult> where TResult : class
	{
		// public ConstructFactory() : this( () => SystemActivator.Instance ) {}

		public ConstructFactory( IActivator activator ) : this( activator, ConstructFactoryParameterCoercer<TResult>.Instance ) {}

		public ConstructFactory( IActivator activator, IFactoryParameterCoercer<ConstructParameter> coercer ) : base( activator, coercer ) {}

		protected override TResult Activate( ConstructParameter parameter ) => Activator.Construct<TResult>( parameter.Type, parameter.Arguments );
	}
}