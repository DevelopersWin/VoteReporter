using DragonSpark.Activation.FactoryModel;
using DragonSpark.Extensions;
using System.Composition;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace DragonSpark.TypeSystem
{
	public class AssemblyInformationFactory : FactoryBase<Assembly, AssemblyInformation>
	{
		[Export]
		public static AssemblyInformationFactory Instance { get; } = new AssemblyInformationFactory();

		readonly static System.Type[] Attributes =
		{
			typeof(AssemblyTitleAttribute),
			typeof(AssemblyProductAttribute),
			typeof(AssemblyCompanyAttribute),
			typeof(AssemblyDescriptionAttribute),
			typeof(AssemblyConfigurationAttribute),
			typeof(AssemblyCopyrightAttribute)
		};

		AssemblyInformationFactory() : base( new FactoryParameterCoercer<Assembly>() ) {}

		protected override AssemblyInformation CreateItem( Assembly parameter )
		{
			var result = new AssemblyInformation { Version = parameter.GetName().Version };
			Attributes.Select( parameter.GetCustomAttribute ).Cast<object>().NotNull().Each( item => item.MapInto( result ) );
			result.Configuration = result.Configuration.NullIfEmpty() ?? TypeSystem.Attributes.Get( parameter ).From<DebuggableAttribute, string>( attribute => "DEBUG" );
			return result;
		}
	}
}