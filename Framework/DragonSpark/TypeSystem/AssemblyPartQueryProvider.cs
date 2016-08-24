using System.Reflection;
using DragonSpark.Aspects.Validation;
using DragonSpark.Extensions;
using DragonSpark.Sources.Parameterized;

namespace DragonSpark.TypeSystem
{
	[ApplyAutoValidation]
	public sealed class AssemblyPartQueryProvider : ValidatedParameterizedSourceBase<Assembly, string>
	{
		public static AssemblyPartQueryProvider Default { get; } = new AssemblyPartQueryProvider();
		AssemblyPartQueryProvider() {}

		public override string Get( Assembly parameter ) => parameter.From<AssemblyPartsAttribute, string>( attribute => attribute.Query ) ?? AssemblyPartsAttribute.Default;
	}
}