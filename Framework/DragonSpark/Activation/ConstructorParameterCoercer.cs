using System;

namespace DragonSpark.Activation
{
	public class ConstructorParameterCoercer<TResult> : TypeRequestCoercer<ConstructTypeRequest, TResult>
	{
		public new static ConstructorParameterCoercer<TResult> Instance { get; } = new ConstructorParameterCoercer<TResult>();

		protected override ConstructTypeRequest Create( Type type, object parameter ) => new ConstructTypeRequest( type, parameter );
	}
}