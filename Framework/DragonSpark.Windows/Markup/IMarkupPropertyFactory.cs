using System;
using DragonSpark.Activation;
using DragonSpark.Sources.Parameterized;

namespace DragonSpark.Windows.Markup
{
	public interface IMarkupPropertyFactory : IValidatedParameterizedSource<IServiceProvider, IMarkupProperty> {}
}