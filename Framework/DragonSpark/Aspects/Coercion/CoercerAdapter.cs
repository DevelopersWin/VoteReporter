using DragonSpark.Sources.Parameterized;

namespace DragonSpark.Aspects.Coercion
{
	sealed class CoercerAdapter<TFrom, TTo> : DelegatedInvocation<TFrom, TTo>, ICoercer
	{
		public CoercerAdapter( IParameterizedSource<TFrom, TTo> coercer ) : base( coercer ) {}
	}
}