using System.Reflection;

namespace DragonSpark.Windows.Markup
{
	public class FieldInfoMarkupPropertyFactory : MarkupPropertyFactory<FieldInfo>
	{
		public static FieldInfoMarkupPropertyFactory Instance { get; } = new FieldInfoMarkupPropertyFactory();

		protected override IMarkupProperty Create( object targetObject, FieldInfo targetProperty ) => new ClrFieldMarkupProperty( targetObject, targetProperty );
	}
}