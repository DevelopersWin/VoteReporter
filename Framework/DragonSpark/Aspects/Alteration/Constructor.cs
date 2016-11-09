using DragonSpark.Sources.Parameterized;
using System;
using DragonSpark.Aspects.Adapters;

namespace DragonSpark.Aspects.Alteration
{
	sealed class Constructor : GenericAdapterConstructorFactory<object, IAlteration>
	{
		public static IParameterizedSource<Type, Func<object, IAlteration>> Default { get; } = new Constructor().ToCache();
		Constructor() : base( typeof(IAlteration<>), typeof(Adapters.AlterationAdapter<>) ) {}
	}
}