using System;
using DragonSpark.Extensions;

namespace DragonSpark.Activation.FactoryModel
{
	public class ActivateFactoryParameterCoercer<TResult> : ActivationFactoryParameterCoercer<ActivateParameter, TResult>
	{
		public new static ActivateFactoryParameterCoercer<TResult> Instance { get; } = new ActivateFactoryParameterCoercer<TResult>();

		// public ActivateFactoryParameterCoercer() : this( Activator.GetCurrent ) {}

		// public ActivateFactoryParameterCoercer( IActivator activator ) : base( activator ) {}

		protected override ActivateParameter Create( Type type, object parameter ) => new ActivateParameter( type, parameter.As<string>() );
	}
}