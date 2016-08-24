using System.Reflection;

namespace DragonSpark.Windows.Markup
{
	public class ClrFieldMarkupProperty : ClrMemberMarkupProperty<FieldInfo>
	{
		public ClrFieldMarkupProperty( object targetObject, FieldInfo targetProperty ) : base( targetProperty, x => targetProperty.SetValue( targetObject, x ), () => targetProperty.GetValue( targetObject ) ) {}
	}
}