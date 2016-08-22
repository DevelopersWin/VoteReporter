using DragonSpark.Sources.Parameterized;
using System;

namespace DragonSpark.Windows.Markup
{
	public interface IMarkupPropertyFactory : IValidatedParameterizedSource<IServiceProvider, IMarkupProperty> {}
}