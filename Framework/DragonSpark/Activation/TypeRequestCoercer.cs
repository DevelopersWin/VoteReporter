using DragonSpark.Extensions;
using System;

namespace DragonSpark.Activation
{
	public abstract class TypeRequestCoercer<TParameter> : CoercerBase<TParameter>
	{
		readonly Func<Type, TParameter> creator;

		protected TypeRequestCoercer()
		{
			creator = Create;
		}

		protected override TParameter PerformCoercion( object parameter ) => parameter.AsTo( creator );

		protected abstract TParameter Create( Type type );
	}
}