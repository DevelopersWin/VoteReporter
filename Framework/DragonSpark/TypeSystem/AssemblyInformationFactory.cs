using DragonSpark.Activation;
using DragonSpark.Extensions;
using System.Composition;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace DragonSpark.TypeSystem
{
	[Export, Shared]
	public class AssemblyInformationFactory : FactoryWithSpecificationBase<Assembly, AssemblyInformation>
	{
		readonly static System.Type[] Attributes =
		{
			typeof(AssemblyTitleAttribute),
			typeof(AssemblyProductAttribute),
			typeof(AssemblyCompanyAttribute),
			typeof(AssemblyDescriptionAttribute),
			typeof(AssemblyConfigurationAttribute),
			typeof(AssemblyCopyrightAttribute)
		};

		public override AssemblyInformation Create( Assembly parameter )
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