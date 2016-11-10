using DragonSpark.Sources.Parameterized;

namespace DragonSpark.Aspects.Adapters
{
	public sealed class AlterationAdapter<T> : DelegatedAdapter<T, T>, IAlterationAdapter
	{
		public AlterationAdapter( IAlteration<T> alteration ) : base( alteration.Get ) {}
	}
}