using DragonSpark.Sources.Parameterized;

namespace DragonSpark.Aspects.Adapters
{
	public sealed class AlterationAdapter<T> : DelegatedInvocation<T, T>, IAlteration
	{
		public AlterationAdapter( IAlteration<T> alteration ) : base( alteration.Get ) {}
	}
}