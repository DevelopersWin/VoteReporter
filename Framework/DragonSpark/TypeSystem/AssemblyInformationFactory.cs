using DragonSpark.Extensions;
using DragonSpark.Sources.Parameterized;
using System;
using System.Composition;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace DragonSpark.TypeSystem
{
	public sealed class DefaultAssemblyInformationSource : FixedFactory<Assembly, AssemblyInformation>
	{
		public static DefaultAssemblyInformationSource Instance { get; } = new DefaultAssemblyInformationSource();
		DefaultAssemblyInformationSource() : base( AssemblyInformationFactory.Instance.Get, ApplicationAssembly.Instance.Get ) {}
	}

	public sealed class AssemblyInformationFactory : ParameterizedSourceBase<Assembly, AssemblyInformation>
	{
		readonly static Type[] Attributes =
		{
			typeof(AssemblyTitleAttribute),
			typeof(AssemblyProductAttribute),
			typeof(AssemblyCompanyAttribute),
			typeof(AssemblyDescriptionAttribute),
			typeof(AssemblyConfigurationAttribute),
			typeof(AssemblyCopyrightAttribute)
		};

		[Export]
		public static IParameterizedSource<Assembly, AssemblyInformation> Instance { get; } = new AssemblyInformationFactory().ToCache();
		AssemblyInformationFactory() {}

		public override AssemblyInformation Get( Assembly parameter )
		{
			var result = new AssemblyInformation { Version = parameter.GetName().Version };
			foreach ( var item in Attributes.Select( parameter.GetCustomAttribute ).WhereAssigned() )
			{
				item.MapInto( result );
			}
			result.Configuration = result.Configuration.NullIfEmpty() ?? parameter.From<DebuggableAttribute, string>( attribute => "DEBUG" );
			return result;
		}
	}
}