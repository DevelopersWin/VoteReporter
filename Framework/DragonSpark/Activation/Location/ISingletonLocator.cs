using System;
using DragonSpark.Sources.Parameterized;

namespace DragonSpark.Activation.Location
{
	public interface ISingletonLocator : IParameterizedSource<Type, object> {}
}