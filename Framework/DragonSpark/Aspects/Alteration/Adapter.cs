using DragonSpark.Sources.Parameterized;

namespace DragonSpark.Aspects.Alteration
{
	sealed class Adapter<T> : DelegatedInvocation<T, T>, IAlteration
	{
		public Adapter( IAlteration<T> alteration ) : base( alteration.Get ) {}
	}
}