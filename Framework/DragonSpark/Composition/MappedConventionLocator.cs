using DragonSpark.Extensions;
using DragonSpark.Sources.Parameterized;
using System;
using System.Reflection;

namespace DragonSpark.Composition
{
	sealed class MappedConventionLocator : AlterationBase<Type>
	{
		public static MappedConventionLocator Default { get; } = new MappedConventionLocator();
		MappedConventionLocator() {}

		public override Type Get( Type parameter )
		{
			var name = $"{parameter.Namespace}.{ConventionCandidateNames.Default.Get( parameter )}";
			var result = name != parameter.FullName ? Get( parameter, name ) : null;
			return result;
		}

		static Type Get( Type parameter, string name )
		{
			var type = parameter.Assembly().GetType( name );
			var result = parameter.GetTypeInfo().IsGenericTypeDefinition == type.GetTypeInfo().IsGenericTypeDefinition ? type : null;
			return result;
		}
	}
}