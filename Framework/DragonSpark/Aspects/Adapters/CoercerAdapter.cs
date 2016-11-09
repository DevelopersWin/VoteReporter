using DragonSpark.Sources.Parameterized;

namespace DragonSpark.Aspects.Adapters
{
	sealed class CoercerAdapter<TFrom, TTo> : DelegatedInvocation<TFrom, TTo>, ICoercer
	{
		public CoercerAdapter( IParameterizedSource<TFrom, TTo> coercer ) : base( coercer ) {}
	}
}