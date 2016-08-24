using System.Reflection;

namespace DragonSpark.Windows.Markup
{
	public class ClrPropertyMarkupProperty : ClrMemberMarkupProperty<PropertyInfo>
	{
		public ClrPropertyMarkupProperty( object targetObject, PropertyInfo targetProperty ) : base( targetProperty, x => targetProperty.SetValue( targetObject, x ), () => targetProperty.GetValue( targetObject ) ) {}
	}
}