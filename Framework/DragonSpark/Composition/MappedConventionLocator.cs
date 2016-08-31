using System;
using DragonSpark.Extensions;
using DragonSpark.Sources.Parameterized;

namespace DragonSpark.Composition
{
	sealed class MappedConventionLocator : AlterationBase<Type>
	{
		public static MappedConventionLocator Default { get; } = new MappedConventionLocator();
		MappedConventionLocator() {}

		public override Type Get( Type parameter )
		{
			var name = $"{parameter.Namespace}.{ConventionCandidateNames.Default.Get( parameter )}";
			var result = name != parameter.FullName ? parameter.Assembly().GetType( name ) : null;
			return result;
		}
	}
}