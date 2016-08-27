using DragonSpark.Sources.Parameterized;
using System;

namespace DragonSpark.Activation
{
	public interface IActivator : IValidatedParameterizedSource<TypeRequest, object>, IValidatedParameterizedSource<Type, object> {}
}