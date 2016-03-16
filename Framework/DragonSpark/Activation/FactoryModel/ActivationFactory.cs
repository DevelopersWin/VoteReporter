using System.ComponentModel.DataAnnotations;

namespace DragonSpark.Activation.FactoryModel
{
	public abstract class ActivationFactory<TParameter, TResult> : FactoryBase<TParameter, TResult> where TParameter : ActivationParameter where TResult : class
	{
		protected ActivationFactory( [Required]IActivator activator, IFactoryParameterCoercer<TParameter> coercer ) : base( coercer )
		{
			Activator = activator;
		}

		protected IActivator Activator { get; }

		protected override TResult CreateItem( TParameter parameter ) => Activate( parameter );

		protected abstract TResult Activate( TParameter parameter );
	}
}