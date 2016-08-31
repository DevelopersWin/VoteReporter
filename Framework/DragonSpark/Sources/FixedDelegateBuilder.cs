using DragonSpark.Sources.Parameterized;
using System;

namespace DragonSpark.Sources
{
	public sealed class FixedDelegateBuilder<T> : AlterationBase<Func<T>>
	{
		public static IParameterizedSource<Func<T>, Func<T>> Default { get; } = new FixedDelegateBuilder<T>()/*.ToCache()*/;
		FixedDelegateBuilder() {}

		public override Func<T> Get( Func<T> parameter ) => new FixedDeferedSource<T>( parameter ).Get;
	}
}