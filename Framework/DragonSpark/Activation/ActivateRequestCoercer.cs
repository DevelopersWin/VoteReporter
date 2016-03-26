using System;
using DragonSpark.Extensions;

namespace DragonSpark.Activation
{
	public class ActivateRequestCoercer<TResult> : TypeRequestCoercer<LocateTypeRequest, TResult>
	{
		public new static ActivateRequestCoercer<TResult> Instance { get; } = new ActivateRequestCoercer<TResult>();

		// public ActivateRequestCoercer() : this( Activator.GetCurrent ) {}

		// public ActivateRequestCoercer( IActivator activator ) : base( activator ) {}

		protected override LocateTypeRequest Create( Type type, object parameter ) => new LocateTypeRequest( type, parameter.As<string>() );
	}
}