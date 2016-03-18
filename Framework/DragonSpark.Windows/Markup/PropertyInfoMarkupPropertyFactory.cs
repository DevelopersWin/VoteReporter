using System.Reflection;

namespace DragonSpark.Windows.Markup
{
	public class PropertyInfoMarkupPropertyFactory : MarkupPropertyFactory<PropertyInfo>
	{
		public static PropertyInfoMarkupPropertyFactory Instance { get; } = new PropertyInfoMarkupPropertyFactory();

		protected override IMarkupProperty Create( object targetObject, PropertyInfo targetProperty ) => new ClrPropertyMarkupProperty( targetObject, targetProperty );
	}
}