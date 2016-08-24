using System;
using DragonSpark.Sources.Parameterized;
using DragonSpark.Specifications;

namespace DragonSpark.Windows.Markup
{
	public abstract class MarkupPropertyFactoryBase : ValidatedParameterizedSourceBase<IServiceProvider, IMarkupProperty>, IMarkupPropertyFactory
	{
		protected MarkupPropertyFactoryBase( ISpecification<IServiceProvider> specification ) : base( specification ) {}
	}
}