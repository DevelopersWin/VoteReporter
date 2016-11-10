using DragonSpark.Sources.Parameterized;

namespace DragonSpark.Aspects.Adapters
{
	sealed class CoercerAdapter<TFrom, TTo> : DelegatedAdapter<TFrom, TTo>, ICoercerAdapter
	{
		public CoercerAdapter( IParameterizedSource<TFrom, TTo> coercer ) : base( coercer ) {}
	}
}