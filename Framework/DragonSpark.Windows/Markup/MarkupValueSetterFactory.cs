using System;
using DragonSpark.Sources.Parameterized;

namespace DragonSpark.Windows.Markup
{
	public class MarkupValueSetterFactory : CompositeFactory<IServiceProvider, IMarkupProperty>, IMarkupPropertyFactory
	{
		public static MarkupValueSetterFactory Default { get; } = new MarkupValueSetterFactory();

		MarkupValueSetterFactory() : base( 
			DependencyPropertyMarkupPropertyFactory.Default, 
			CollectionMarkupPropertyFactory.Default, 
			PropertyInfoMarkupPropertyFactory.Default, 
			FieldInfoMarkupPropertyFactory.Default ) {}
	}
}