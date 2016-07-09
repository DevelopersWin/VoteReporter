using DragonSpark.Aspects.Validation;
using System.Windows;

namespace DragonSpark.Windows.Markup
{
	[ApplyAutoValidation]
	public class DependencyPropertyMarkupPropertyFactory : MarkupPropertyFactoryBase<DependencyObject, DependencyProperty>
	{
		public static DependencyPropertyMarkupPropertyFactory Instance { get; } = new DependencyPropertyMarkupPropertyFactory();

		protected override IMarkupProperty Create( DependencyObject targetObject, DependencyProperty targetProperty ) => new DependencyPropertyMarkupProperty( targetObject, targetProperty );

		//protected override Type GetPropertyType( DependencyObject target, DependencyProperty property ) => property.PropertyType;
	}
}