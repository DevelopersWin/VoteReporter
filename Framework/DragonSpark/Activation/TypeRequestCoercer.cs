using System;
using DragonSpark.Extensions;

namespace DragonSpark.Activation
{
	public abstract class TypeRequestCoercer<TParameter> : ParameterCoercer<TParameter>
	{
		protected override TParameter PerformCoercion( object context ) => context.AsTo<Type, TParameter>( Create );

		protected abstract TParameter Create( Type type );
	}
}