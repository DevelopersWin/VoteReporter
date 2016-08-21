using DragonSpark.Aspects.Validation;
using System.Reflection;

namespace DragonSpark.Windows.Markup
{
	[ApplyAutoValidation]
	public class FieldInfoMarkupPropertyFactory : MarkupPropertyFactory<FieldInfo>
	{
		public static FieldInfoMarkupPropertyFactory Default { get; } = new FieldInfoMarkupPropertyFactory();

		protected override IMarkupProperty Create( object targetObject, FieldInfo targetProperty ) => new ClrFieldMarkupProperty( targetObject, targetProperty );
	}
}