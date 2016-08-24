using System.Reflection;
using DragonSpark.Aspects.Validation;
using DragonSpark.Extensions;
using DragonSpark.Sources.Parameterized;

namespace DragonSpark.TypeSystem
{
	[ApplyAutoValidation]
	public sealed class AssemblyHintProvider : ValidatedParameterizedSourceBase<Assembly, string>
	{
		public static AssemblyHintProvider Default { get; } = new AssemblyHintProvider();
		AssemblyHintProvider() {}

		public override string Get( Assembly parameter ) => parameter.From<AssemblyHintAttribute, string>( attribute => attribute.Hint ) ?? parameter.GetName().Name;
	}
}