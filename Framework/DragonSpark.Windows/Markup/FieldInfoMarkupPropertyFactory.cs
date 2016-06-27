using System.Reflection;
using DragonSpark.Aspects;

namespace DragonSpark.Windows.Markup
{
	[AutoValidation.GenericFactory]
	public class FieldInfoMarkupPropertyFactory : MarkupPropertyFactory<FieldInfo>
	{
		public static FieldInfoMarkupPropertyFactory Instance { get; } = new FieldInfoMarkupPropertyFactory();

		protected override IMarkupProperty Create( object targetObject, FieldInfo targetProperty ) => new ClrFieldMarkupProperty( targetObject, targetProperty );
	}
}