using DragonSpark.Aspects.Adapters;
using DragonSpark.Sources.Parameterized;
using DragonSpark.Sources.Parameterized.Caching;
using System;

namespace DragonSpark.Aspects.Alteration
{
	sealed class Constructor : GenericAdapterConstructorFactory<object, IAlterationAdapter>
	{
		public static IParameterizedSource<Type, Func<object, IAlterationAdapter>> Default { get; } = new Constructor().ToCache();
		Constructor() : base( typeof(IAlteration<>), typeof(Adapters.AlterationAdapter<>) ) {}
	}
}