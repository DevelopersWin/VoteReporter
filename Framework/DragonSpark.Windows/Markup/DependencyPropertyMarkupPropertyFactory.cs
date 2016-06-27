using System.Windows;
using DragonSpark.Aspects;

namespace DragonSpark.Windows.Markup
{
	[AutoValidation.GenericFactory]
	public class DependencyPropertyMarkupPropertyFactory : MarkupPropertyFactoryBase<DependencyObject, DependencyProperty>
	{
		public static DependencyPropertyMarkupPropertyFactory Instance { get; } = new DependencyPropertyMarkupPropertyFactory();

		protected override IMarkupProperty Create( DependencyObject targetObject, DependencyProperty targetProperty ) => new DependencyPropertyMarkupProperty( targetObject, targetProperty );

		//protected override Type GetPropertyType( DependencyObject target, DependencyProperty property ) => property.PropertyType;
	}
}