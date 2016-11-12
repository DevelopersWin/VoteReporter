using DragonSpark.Sources.Parameterized;
using System;

namespace DragonSpark.Sources
{
	public sealed class SingletonDelegateAlteration<T> : AlterationBase<Func<T>>
	{
		public static IParameterizedSource<Func<T>, Func<T>> Default { get; } = new SingletonDelegateAlteration<T>();
		SingletonDelegateAlteration() {}

		public override Func<T> Get( Func<T> parameter ) => new DelegatedSingletonSource<T>( parameter ).Get;
	}
}