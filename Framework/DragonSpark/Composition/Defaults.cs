using DragonSpark.Extensions;
using DragonSpark.Sources.Parameterized;
using System;
using System.Composition;
using System.Reflection;

namespace DragonSpark.Composition
{
	sealed class ExportLocator : ParameterizedSourceBase<Type, ExportMapping>
	{
		public static IParameterizedSource<Type, ExportMapping> Default { get; } = new ExportLocator().ToCache();
		ExportLocator() {}

		public override ExportMapping Get( Type parameter )
		{
			foreach ( var candidate in parameter.GetTypeInfo().Append<MemberInfo>( ExportedSingletonProperties.Default.Get( parameter ) ).WhereAssigned() )
			{
				var attribute = candidate.GetAttribute<ExportAttribute>();
				if ( attribute != null )
				{
					var result = new ExportMapping( parameter, attribute.ContractType ?? candidate.GetMemberType() );
					return result;
				}
			}

			return null;
		}
	}
}