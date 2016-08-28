using DragonSpark.Activation.Location;
using DragonSpark.Extensions;
using DragonSpark.Sources.Parameterized;
using System;
using System.Composition;
using System.Reflection;

namespace DragonSpark.Composition
{
	sealed class DefinedExportLocator : ParameterizedSourceBase<Type, ExportedItemDescriptor>
	{
		public static IParameterizedSource<Type, ExportedItemDescriptor> Default { get; } = new DefinedExportLocator().ToCache();
		DefinedExportLocator() {}

		public override ExportedItemDescriptor Get( Type parameter )
		{
			foreach ( var candidate in parameter.GetTypeInfo().Append<MemberInfo>( SingletonProperties.Default.Get( parameter ) ).WhereAssigned() )
			{
				var attribute = candidate.GetAttribute<ExportAttribute>();
				if ( attribute != null )
				{
					var result = new ExportedItemDescriptor( parameter, candidate, attribute.ContractType ?? candidate.GetMemberType() );
					return result;
				}
			}

			return default(ExportedItemDescriptor);
		}
	}

	public struct ExportedItemDescriptor
	{
		public ExportedItemDescriptor( Type subject, MemberInfo location, Type exportAs )
		{
			Subject = subject;
			Location = location;
			ExportAs = exportAs;
		}

		public Type Subject {get; }

		public MemberInfo Location { get; }

		public Type ExportAs { get; }
	}
}