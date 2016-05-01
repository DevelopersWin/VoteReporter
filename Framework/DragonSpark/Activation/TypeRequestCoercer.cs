using DragonSpark.Extensions;
using System;

namespace DragonSpark.Activation
{
	public abstract class TypeRequestCoercer<TParameter> : CoercerBase<TParameter>
	{
		protected override TParameter PerformCoercion( object parameter ) => parameter.AsTo<Type, TParameter>( Create );

		protected abstract TParameter Create( Type type );
	}
}