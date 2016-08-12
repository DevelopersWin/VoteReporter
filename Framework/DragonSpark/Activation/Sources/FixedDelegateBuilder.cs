using System;

namespace DragonSpark.Activation.Sources
{
	public sealed class FixedDelegateBuilder<T> : TransformerBase<Func<T>>
	{
		public static IParameterizedSource<Func<T>, Func<T>> Instance { get; } = new FixedDelegateBuilder<T>()/*.ToCache()*/;
		FixedDelegateBuilder() {}

		public override Func<T> Get( Func<T> parameter ) => new FixedDeferedSource<T>( parameter ).Get;
	}
}